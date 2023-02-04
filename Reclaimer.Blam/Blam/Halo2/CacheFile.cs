using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;
using System.IO;

namespace Reclaimer.Blam.Halo2
{
    public class CacheFile : ICacheFile
    {
        public const string MainMenuMap = "mainmenu.map";
        public const string SharedMap = "shared.map";
        public const string SinglePlayerSharedMap = "single_player_shared.map";

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

        [Offset(36)]
        [VersionNumber]
        public int Version { get; set; }

        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public int IndexAddress { get; set; }

        [Offset(20)]
        public int IndexSize { get; set; }

        [Offset(288, MinVersion = 0)]
        [Offset(300, MaxVersion = 0)]
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

        [Offset(444, MinVersion = 0)]
        [Offset(456, MaxVersion = 0)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }

        [Offset(704, MinVersion = 0)]
        [Offset(716, MaxVersion = 0)]
        public int FileCount { get; set; }

        [Offset(708, MinVersion = 0)]
        [Offset(720, MaxVersion = 0)]
        public int FileTableAddress { get; set; }

        [Offset(712, MinVersion = 0)]
        [Offset(724, MaxVersion = 0)]
        public int FileTableSize { get; set; }

        [Offset(716, MinVersion = 0)]
        [Offset(728, MaxVersion = 0)]
        public int FileTableIndexOffset { get; set; }
    }

    [FixedSize(32)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        public static int HeaderSize => 32;

        internal Dictionary<int, string> TagNames { get; }

        [Offset(0)]
        public int Magic { get; set; }

        [Offset(4)]
        public int TagClassCount { get; set; }

        [Offset(8)]
        public Pointer TagDataAddress { get; set; }

        [Offset(24)]
        public int TagCount { get; set; }

        public TagIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new Dictionary<int, IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();
            TagNames = new Dictionary<int, string>();
        }

        internal void ReadItems()
        {
            if (items.Any())
                throw new InvalidOperationException();

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.Seek(TagDataAddress.Address, SeekOrigin.Begin);
                for (var i = 0; i < TagCount; i++)
                {
                    //Halo2Vista multiplayer maps have empty tags in them
                    var item = reader.ReadObject(new IndexItem(cache));
                    if (item.Id < 0)
                        continue;

                    items.Add(i, item);

                    //Halo2Vista multiplayer maps have two ugh! tags
                    if (CacheFactory.SystemClasses.Contains(item.ClassCode) && !sysItems.ContainsKey(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                reader.Seek(cache.Header.FileTableIndexOffset, SeekOrigin.Begin);
                var indices = reader.ReadArray<int>(TagCount);

                for (var i = 0; i < TagCount; i++)
                {
                    reader.Seek(cache.Header.FileTableAddress + indices[i], SeekOrigin.Begin);
                    TagNames.Add(i, reader.ReadNullTerminatedString());
                }
            }
        }

        public IndexItem GetGlobalTag(string classCode) => sysItems[classCode];

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.Values.GetEnumerator();
    }

    public class StringIndex : IStringIndex
    {
        private readonly CacheFile cache;
        private readonly string[] items;

        public StringIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new string[cache.Header.StringCount];
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
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
                        items[i] = reader2.ReadNullTerminatedString();
                    }
                }
            }
        }

        public int StringCount => items.Length;

        public string this[int id] => items[id];

        public int GetStringId(string value) => Array.IndexOf(items, value);

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
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
        [StoreType(typeof(short))]
        public int Id { get; set; }

        [Offset(8)]
        public Pointer MetaPointer { get; set; }

        [Offset(12)]
        public int MetaSize { get; set; }

        public string ClassCode => System.Text.Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId).Reverse().ToArray());

        public string ClassName => CacheFactory.Halo2Classes.TryGetValue(ClassCode, out var className) ? className : ClassCode;

        public string TagName => cache.TagIndex.TagNames[Id];

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
                        metadataCache = lazy = new Lazy<T>(ReadMetadataInternal<T>);
                }

                return lazy.Value;
            }
            else
                return ReadMetadataInternal<T>();
        }

        private T ReadMetadataInternal<T>()
        {
            long address;
            DependencyReader reader;

            if (ClassCode == "sbsp")
            {
                var translator = new BSPAddressTranslator(cache, Id);
                reader = cache.CreateReader(translator);
                address = translator.TagAddress;
            }
            else
            {
                reader = cache.CreateReader(cache.MetadataTranslator);
                address = MetaPointer.Address;
            }

            using (reader)
            {
                reader.RegisterInstance<IIndexItem>(this);
                reader.Seek(address, SeekOrigin.Begin);
                var result = (T)reader.ReadObject(typeof(T), (int)cache.CacheType);

                if (CacheFactory.SystemClasses.Contains(ClassCode))
                    metadataCache = result;

                return result;
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {TagName}");
    }
}
