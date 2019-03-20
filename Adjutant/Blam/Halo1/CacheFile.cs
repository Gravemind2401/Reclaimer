using Adjutant.Blam.Definitions;
using Adjutant.IO;
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
        public string FileName { get; }
        public string BuildString => Header.BuildString;
        public CacheType Type => Header.CacheType;

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }

        public scenario Scenario { get; }

        public TagAddressTranslator AddressTranslator { get; }

        public CacheFile(string fileName)
        {
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

        public DependencyReader CreateReader(IAddressTranslator translator)
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder.LittleEndian);
            reader.RegisterType<CacheFile>(() => this);
            reader.RegisterType<Pointer>(() => new Pointer(reader.ReadInt32(), translator));
            reader.RegisterType<IAddressTranslator>(() => translator);
            return reader;
        }

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => null;

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

        public CacheType CacheType
        {
            get
            {
                switch (Version)
                {
                    case 5: return CacheType.Halo1Xbox;
                    case 7: return CacheType.Halo1PC;
                    case 609: return CacheType.Halo1CE;
                    default: return CacheType.Unknown;
                }
            }
        }
    }

    [FixedSize(40)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly List<IndexItem> items;
        private readonly Dictionary<int, string> filenames;

        internal Dictionary<int, string> Filenames => filenames;

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
            filenames = new Dictionary<int, string>();
        }

        internal void ReadItems(DependencyReader reader)
        {
            if (items.Any())
                throw new InvalidOperationException();

            for (int i = 0; i < TagCount; i++)
            {
                reader.Seek(cache.Header.IndexAddress + 40 + i * 32, SeekOrigin.Begin);

                var item = reader.ReadObject(new IndexItem(cache));
                items.Add(item);

                reader.Seek(item.FileNamePointer.Address, SeekOrigin.Begin);
                filenames.Add(item.Id, reader.ReadNullTerminatedString());
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

        public IndexItem(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        [ByteOrder(ByteOrder.BigEndian)]
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

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId)).TrimEnd();

        public string FileName => cache.TagIndex.Filenames[Id];

        public T ReadMetadata<T>()
        {
            if (typeof(T).Equals(typeof(scenario_structure_bsp)))
            {
                var translator = new BSPAddressTranslator(cache, Id);
                using (var reader = cache.CreateReader(translator))
                {
                    reader.Seek(translator.TagAddress, SeekOrigin.Begin);
                    return (T)(object)reader.ReadObject<scenario_structure_bsp>(cache.Header.Version);
                }
            }

            using (var reader = cache.CreateReader(cache.AddressTranslator))
            {
                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                return (T)reader.ReadObject(typeof(T), cache.Header.Version);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FileName}");
        }
    }
}
