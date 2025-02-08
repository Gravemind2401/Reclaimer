using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;
using System.IO;
using System.Text;

namespace Reclaimer.Blam.Halo1
{
    public class CacheFile : ICacheFile
    {
        public const string BitmapsMap = "bitmaps.map";

        private readonly long fileSize;

        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }
        public CacheMetadata Metadata { get; }

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }

        public TagAddressTranslator AddressTranslator { get; }

        public bool IsCompressed => CacheType == CacheType.Halo1Xbox && Header?.FileSize > fileSize;

        public CacheFile(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFile(CacheArgs args)
        {
            Exceptions.ThrowIfFileNotFound(args.FileName);

            fileSize = new FileInfo(args.FileName).Length;

            FileName = args.FileName;
            ByteOrder = args.ByteOrder;
            BuildString = args.BuildString;
            CacheType = args.CacheType;
            Metadata = args.Metadata;

            AddressTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(AddressTranslator))
                Header = reader.ReadObject<CacheHeader>();

            //TODO: make the decompressor work for bitmaps.map, otherwise its not very useful
            if (IsCompressed)
                throw new NotSupportedException("This map file needs to be decompressed before it can be opened");

            //start a new reader - if the file appears to be compressed then the new reader will decompress as it reads
            using (var reader = CreateReader(AddressTranslator))
            {
                reader.Seek(Header.IndexAddress, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this), (int)CacheType);
                TagIndex.ReadItems(reader);
            }
        }

        private DependencyReader CreateReader(IAddressTranslator translator)
        {
            ArgumentNullException.ThrowIfNull(translator);

            var reader = CacheFactory.CreateReader(this, translator);
            reader.RegisterInstance<CacheFile>(this);
            return reader;
        }

        internal DependencyReader CreateBitmapsReader()
        {
            var bitmapsMap = BlamUtils.FindResourceFile(this, BitmapsMap)
                ?? throw new FileNotFoundException($"Could not find {BitmapsMap}");

            var fs = (Stream)new FileStream(bitmapsMap, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder);

            reader.RegisterInstance<CacheFile>(this);
            reader.RegisterInstance<ICacheFile>(this);

            return reader;
        }

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => null;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => AddressTranslator;

        #endregion
    }

    [FixedSize(2048)]
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
        private readonly Dictionary<string, IndexItem> sysItems;

        public int HeaderSize => cache.CacheType == CacheType.Halo1Xbox ? 36 : 40;

        internal Dictionary<int, string> TagNames { get; }

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
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new List<IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();
            TagNames = new Dictionary<int, string>();
        }

        internal void ReadItems(DependencyReader reader)
        {
            if (items.Count > 0)
                throw new InvalidOperationException();

            for (var i = 0; i < TagCount; i++)
            {
                reader.Seek(cache.Header.IndexAddress + HeaderSize + i * 32, SeekOrigin.Begin);

                var item = reader.ReadObject(new IndexItem(cache));
                items.Add(item);

                if (CacheFactory.SystemClasses.Contains(item.ClassCode))
                    sysItems.Add(item.ClassCode, item);

                reader.Seek(item.FileNamePointer.Address, SeekOrigin.Begin);
                TagNames.Add(item.Id, reader.ReadNullTerminatedString());
            }
        }

        public IndexItem GetGlobalTag(string classCode) => sysItems[classCode];

        int ITagIndexGen1.Magic => Magic - (cache.Header.IndexAddress + cache.TagIndex.HeaderSize);

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(32)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

        private object metadataCache;

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

        public string ClassName => CacheFactory.Halo1Classes.TryGetValue(ClassCode, out var className) ? className : ClassCode;

        public string TagName => cache.TagIndex.TagNames[Id];

        public T ReadMetadata<T>()
        {
            if (metadataCache?.GetType() == typeof(T))
                return (T)metadataCache;

            long address;
            DependencyReader reader;

            if (ClassCode == "sbsp")
            {
                var translator = new BspAddressTranslator(cache, Id);
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
                var result = reader.ReadObject<T>((int)cache.CacheType);

                if (CacheFactory.SystemClasses.Contains(ClassCode))
                    metadataCache = result;

                return result;
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {TagName}");
    }
}
