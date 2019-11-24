using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class CacheFile : ICacheFile
    {
        public const string BitmapsMap = "bitmaps.map";

        public string FileName { get; }
        public string BuildString => Header.BuildString;
        public CacheType CacheType => CacheFactory.GetCacheTypeByBuild(BuildString);

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }

        public scenario Scenario { get; }

        public TagAddressTranslator AddressTranslator { get; }

        public CacheFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            FileName = fileName;
            AddressTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(AddressTranslator))
            {
                Header = reader.ReadObject<CacheHeader>();

                reader.Seek(Header.IndexAddress, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                TagIndex.ReadItems(reader);
            }

            Scenario = TagIndex.FirstOrDefault(t => t.ClassCode == "scnr")?.ReadMetadata<scenario>();
        }

        private DependencyReader CreateReader(string fileName, IAddressTranslator translator, bool headerCheck)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder.LittleEndian);

            var header = reader.PeekInt32();
            if (header == CacheFactory.BigHeader)
                reader.ByteOrder = ByteOrder.BigEndian;
            else if (headerCheck && header != CacheFactory.LittleHeader)
                throw Exceptions.NotAValidMapFile(fileName);

            reader.RegisterInstance<CacheFile>(this);
            reader.RegisterInstance<ICacheFile>(this);

            if (translator != null)
                reader.RegisterInstance<IAddressTranslator>(translator);

            return reader;
        }

        public DependencyReader CreateReader(IAddressTranslator translator)
        {
            if (translator == null)
                throw new ArgumentNullException(nameof(translator));

            return CreateReader(FileName, translator, true);
        }

        internal DependencyReader CreateBitmapsReader()
        {
            var folder = Directory.GetParent(FileName).FullName;
            var bitmapsMap = Path.Combine(folder, BitmapsMap);

            return CreateReader(bitmapsMap, null, false);
        }

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => null;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => AddressTranslator;

        #endregion
    }

    public class CacheHeader
    {
        [Offset(0)]
        public int Head { get; set; }

        [Offset(4)]
        [VersionNumber]
        public int Version { get; set; }

        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public int IndexAddress { get; set; }

        [Offset(64)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }
    }

    [FixedSize(36, MaxVersion = (int)CacheType.Halo1PC)]
    [FixedSize(40, MinVersion = (int)CacheType.Halo1PC)]
    public class TagIndex : ITagIndex<IndexItem>, ITagIndexGen1
    {
        private readonly CacheFile cache;
        private readonly List<IndexItem> items;

        public int HeaderSize => cache.CacheType == CacheType.Halo1Xbox ? 36 : 40;

        internal Dictionary<int, string> Filenames { get; }

        int ITagIndexGen1.Magic => Magic - (cache.Header.IndexAddress + cache.TagIndex.HeaderSize);

        [Offset(0)]
        public int Magic { get; set; }

        [Offset(12)]
        public int TagCount { get; set; }

        [Offset(16)]
        public int VertexDataCount { get; set; }

        [Offset(20)]
        public int VertexDataOffset { get; set; }

        [Offset(24)]
        public int IndexDataCount { get; set; }

        [Offset(28)]
        public int IndexDataOffset { get; set; }

        public TagIndex(CacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            items = new List<IndexItem>();
            Filenames = new Dictionary<int, string>();
        }

        internal void ReadItems(DependencyReader reader)
        {
            if (items.Any())
                throw new InvalidOperationException();

            for (int i = 0; i < TagCount; i++)
            {
                reader.Seek(cache.Header.IndexAddress + HeaderSize + i * 32, SeekOrigin.Begin);

                var item = reader.ReadObject(new IndexItem(cache));
                items.Add(item);

                reader.Seek(item.FileNamePointer.Address, SeekOrigin.Begin);
                Filenames.Add(item.Id, reader.ReadNullTerminatedString());
            }
        }

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(32)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

        public IndexItem(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        public int ClassId { get; set; }

        //parent class codes here

        [Offset(12)]
        [StoreType(typeof(ushort))]
        public int Id { get; set; }

        [Offset(16)]
        public Pointer FileNamePointer { get; set; }

        [Offset(20)]
        public Pointer MetaPointer { get; set; }

        [Offset(24)]
        public int Unknown1 { get; set; }

        [Offset(28)]
        public int Unknown2 { get; set; }

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId).Reverse().ToArray());

        public string ClassName
        {
            get
            {
                if (CacheFactory.Halo1Classes.ContainsKey(ClassCode))
                    return CacheFactory.Halo1Classes[ClassCode];

                return ClassCode;
            }
        }

        public string FullPath => cache.TagIndex.Filenames[Id];

        public T ReadMetadata<T>()
        {
            long address;
            DependencyReader reader;

            if (ClassCode == "sbsp")
            {
                var translator = new BSPAddressTranslator(cache, Id);
                reader = cache.CreateReader(translator);
                address = translator.TagAddress;
            }
            else if (ClassCode == "bitm" && MetaPointer.Address < 0)
            {
                reader = cache.CreateBitmapsReader();
                var translator = new BitmapsAddressTranslator(cache, this, reader);
                reader.RegisterInstance<IAddressTranslator>(translator);
                address = translator.TagAddress;
            }
            else
            {
                reader = cache.CreateReader(cache.AddressTranslator);
                address = MetaPointer.Address;
            }

            using (reader)
            {
                reader.RegisterInstance<IIndexItem>(this);
                reader.Seek(address, SeekOrigin.Begin);
                return reader.ReadObject<T>(cache.Header.Version);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FullPath}");
        }
    }
}
