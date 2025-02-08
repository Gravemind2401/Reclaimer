using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;
using System.IO;
using System.Text;

namespace Reclaimer.Blam.Halo2Beta
{
    public class CacheFile : ICacheFile
    {
        public const string MainMenuMap = "mainmenu.map";
        public const string SharedMap = "shared.map";

        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }
        public CacheMetadata Metadata { get; }

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }

        public HeaderAddressTranslator HeaderTranslator { get; }
        public TagAddressTranslator MetadataTranslator { get; }

        public CacheFile(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFile(CacheArgs args)
        {
            Exceptions.ThrowIfFileNotFound(args.FileName);

            FileName = args.FileName;
            ByteOrder = args.ByteOrder;
            BuildString = args.BuildString;
            CacheType = args.CacheType;
            Metadata = args.Metadata;

            HeaderTranslator = new HeaderAddressTranslator(this);
            MetadataTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(HeaderTranslator))
            {
                Header = reader.ReadObject<CacheHeader>();
                reader.Seek(Header.IndexAddress, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();
            }
        }

        public DependencyReader CreateReader(IAddressTranslator translator) => CacheFactory.CreateReader(this, translator);

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        #endregion
    }

    [FixedSize(2048)]
    public class CacheHeader
    {
        [Offset(0)]
        public int Head { get; set; }

        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public int IndexAddress { get; set; }

        [Offset(20)]
        public int IndexSize { get; set; }

        [Offset(288)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }

        [Offset(356)]
        public int StringCount { get; set; }

        [Offset(360)]
        public int StringTableSize { get; set; }

        [Offset(364)]
        public int StringTableIndexAddress { get; set; }

        [Offset(368)]
        public int StringTableAddress { get; set; }

        [Offset(408)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }
    }

    [FixedSize(20)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        public static int HeaderSize => 20;

        [Offset(0)]
        public int Magic { get; set; }

        [Offset(4)]
        public int ScenarioId { get; set; }

        [Offset(12)]
        public int TagCount { get; set; }

        public TagIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new Dictionary<int, IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();
        }

        internal void ReadItems()
        {
            if (items.Any())
                throw new InvalidOperationException();

            using var reader = cache.CreateReader(cache.MetadataTranslator);

            reader.Seek(cache.Header.IndexAddress + HeaderSize, SeekOrigin.Begin);
            for (var i = 0; i < TagCount; i++)
            {
                var item = reader.ReadObject(new IndexItem(cache));
                items.Add(i, item);

                if (CacheFactory.SystemClasses.Contains(item.ClassCode))
                    sysItems[item.ClassCode] = item;
            }

            for (var i = 0; i < TagCount; i++)
            {
                var item = items[i];

                //change FileNamePointer to use HeaderTranslator instead of MetadataTranslator
                item.FileNamePointer = new Pointer(item.FileNamePointer.Value, cache.HeaderTranslator);

                reader.Seek(item.FileNamePointer.Address, SeekOrigin.Begin);
                item.TagName = reader.ReadNullTerminatedString();
            }
        }

        public IndexItem GetGlobalTag(string classCode) => sysItems[classCode];

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.Values.GetEnumerator();
    }

    public class StringIndex : StringIndexBase
    {
        private readonly CacheFile cache;

        public StringIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Items = new string[cache.Header.StringCount];
        }

        internal void ReadItems()
        {
            using var reader = cache.CreateReader(cache.HeaderTranslator);

            var indices = new int[cache.Header.StringCount];
            reader.Seek(cache.Header.StringTableIndexAddress, SeekOrigin.Begin);
            for (var i = 0; i < cache.Header.StringCount; i++)
                indices[i] = reader.ReadInt32();

            using (var reader2 = reader.CreateVirtualReader(cache.Header.StringTableAddress))
            {
                for (var i = 0; i < cache.Header.StringCount; i++)
                {
                    if (indices[i] < 0)
                        continue;

                    reader2.Seek(indices[i], SeekOrigin.Begin);
                    Items[i] = reader2.ReadNullTerminatedString();
                }
            }
        }

        public override int GetStringId(string value) => Array.IndexOf(Items, value);
    }

    [FixedSize(32)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

        private readonly object cacheLock = new object();
        private object metadataCache;

        public IndexItem(CacheFile cache)
        {
            this.cache = cache;
        }

        [Offset(0)]
        public int ClassId { get; set; }

        [Offset(4)]
        public int ParentClassId { get; set; }

        [Offset(8)]
        public int ParentClassId2 { get; set; }

        [Offset(12)]
        [StoreType(typeof(short))]
        public int Id { get; set; }

        [Offset(16)]
        public Pointer FileNamePointer { get; set; }

        [Offset(20)]
        public Pointer MetaPointer { get; set; }

        [Offset(24)]
        public int MetaSize { get; set; }

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId).Reverse().ToArray());

        public string ClassName => CacheFactory.Halo2Classes.TryGetValue(ClassCode, out var className) ? className : ClassCode;

        public string TagName { get; internal set; }

        public IAddressTranslator GetAddressTranslator()
        {
            return ClassCode == "sbsp"
                ? new BSPAddressTranslator(cache, Id)
                : cache.MetadataTranslator;
        }

        public long GetBaseAddress()
        {
            return ClassCode == "sbsp"
                ? new BSPAddressTranslator(cache, Id).TagAddress + 16
                : MetaPointer.Address;
        }

        public T ReadMetadata<T>()
        {
            if (metadataCache is Lazy<T> lazy)
                return lazy.Value;
            else if (CacheFactory.SystemClasses.Contains(ClassCode))
            {
                lock (cacheLock)
                {
                    lazy = metadataCache as Lazy<T>;
                    if (lazy != null)
                        return lazy.Value;
                    else
                        metadataCache = lazy = new Lazy<T>(ReadMetadataInternal);
                }

                return lazy.Value;
            }
            else
                return ReadMetadataInternal();

            T ReadMetadataInternal()
            {
                using var reader = cache.CreateReader(GetAddressTranslator());

                reader.RegisterInstance<IIndexItem>(this);
                reader.Seek(GetBaseAddress(), SeekOrigin.Begin);
                var result = reader.ReadObject<T>((int)cache.CacheType);

                if (CacheFactory.SystemClasses.Contains(ClassCode))
                    metadataCache = result;

                return result;
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {TagName}");
    }
}
