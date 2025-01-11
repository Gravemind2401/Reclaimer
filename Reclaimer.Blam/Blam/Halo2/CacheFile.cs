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
        public const string MccTextureFile = "textures.dat";

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
                Header = reader.ReadObject<CacheHeader>((int)args.CacheType);

                if (Header.Flags.HasFlag(CacheFlags.Compressed))
                    throw new NotSupportedException("Map must be decompressed first");

                if (Header.MetadataAddressMask != 0)
                    System.Diagnostics.Debugger.Break();

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

    public partial class CacheHeader
    {
        public int Head { get; set; }
        public int FileSize { get; set; }
        public int IndexAddress { get; set; }
        public int MetadataOffset { get; set; }
        public int MetadataSize { get; set; }
        public int IndexSize { get; set; }
        public string BuildString { get; set; }
        public int StringCount { get; set; }
        public int StringTableSize { get; set; }
        public int StringTableIndexAddress { get; set; }
        public int StringTableAddress { get; set; }
        public string ScenarioName { get; set; }
        public int FileCount { get; set; }
        public int FileTableAddress { get; set; }
        public int FileTableSize { get; set; }
        public int FileTableIndexAddress { get; set; }

        public int RawTableAddress { get; set; }
        public int RawTableSize { get; set; }

        //MCC fields
        public CacheFlags Flags { get; set; }
        public int MetadataAddressMask { get; set; }
        public int CompressedDataChunkSize { get; set; }
        public int CompressedDataOffset { get; set; }
        public int CompressedChunkTableOffset { get; set; }
        public int CompressedChunkCount { get; set; }
    }

    [Flags]
    public enum CacheFlags : short
    {
        None = 0,
        Compressed = 1
    }

    [FixedSize(32)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        public const int HeaderSize = 32;

        internal Dictionary<int, string> TagNames { get; }

        [Offset(0)]
        public Pointer TagClassOffset { get; set; }

        [Offset(4)]
        public int TagClassCount { get; set; }

        [Offset(8)]
        public Pointer TagDataOffset { get; set; }

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
                reader.Seek(TagDataOffset.Address, SeekOrigin.Begin);
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

                reader.Seek(cache.Header.FileTableIndexAddress, SeekOrigin.Begin);
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
                        Items[i] = reader2.ReadNullTerminatedString();
                    }
                }
            }
        }

        public override int GetStringId(string value) => Array.IndexOf(Items, value);
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

        public IAddressTranslator GetAddressTranslator()
        {
            return cache.Metadata.Platform == CachePlatform.Xbox && ClassCode == "sbsp"
                ? new BSPAddressTranslator(cache, Id)
                : cache.MetadataTranslator;
        }

        public long GetBaseAddress()
        {
            //not sure what the first 16 bytes after the bsp address are but apparently not part of the metadata
            return cache.Metadata.Platform == CachePlatform.Xbox && ClassCode == "sbsp"
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
                using (var reader = cache.CreateReader(GetAddressTranslator()))
                {
                    reader.RegisterInstance<IIndexItem>(this);
                    reader.Seek(GetBaseAddress(), SeekOrigin.Begin);
                    var result = reader.ReadObject<T>((int)cache.CacheType);

                    if (CacheFactory.SystemClasses.Contains(ClassCode))
                        metadataCache = result;

                    return result;
                }
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {TagName}");
    }
}
