using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.MccHaloReach
{
    public class CacheFile : IMccCacheFile
    {
        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }
        public CacheMetadata Metadata { get; }

        public virtual CacheHeader Header { get; }
        public virtual TagIndex TagIndex { get; }
        public virtual StringIndex StringIndex { get; }
        public virtual LocaleIndex LocaleIndex { get; }

        public virtual SectionAddressTranslator HeaderTranslator { get; }
        public virtual TagAddressTranslator MetadataTranslator { get; }

        public virtual PointerExpander PointerExpander { get; }

        protected CacheFile(string fileName, ByteOrder byteOrder, string buildString, CacheType cacheType, CacheMetadata metadata)
        {
            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            FileName = fileName;
            ByteOrder = byteOrder;
            BuildString = buildString;
            CacheType = cacheType;
            Metadata = metadata;
        }

        public CacheFile(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFile(CacheArgs args)
            : this(args.FileName, args.ByteOrder, args.BuildString, args.CacheType, args.Metadata)
        {
            HeaderTranslator = new SectionAddressTranslator(this, 0);
            MetadataTranslator = new TagAddressTranslator(this);
            PointerExpander = new PointerExpander(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeader>((int)CacheType);

            //change IndexPointer to use MetadataTranslator instead of HeaderTranslator
            Header.IndexPointer = new Pointer64(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();

                LocaleIndex = new LocaleIndex(this, 664, 80, 12);
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<HaloReach.cache_file_resource_gestalt>();
                TagIndex.GetGlobalTag("play")?.ReadMetadata<HaloReach.cache_file_resource_layout_table>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<HaloReach.scenario>();
            });
        }

        public DependencyReader CreateReader(IAddressTranslator translator) => CacheFactory.CreateReader(this, translator);

        public DependencyReader CreateReader(IAddressTranslator translator, IPointerExpander expander)
        {
            var reader = CreateReader(translator);
            reader.RegisterInstance(expander);
            return reader;
        }

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        IGen3Header IGen3CacheFile.Header => Header;
        ILocaleIndex IGen3CacheFile.LocaleIndex => LocaleIndex;
        bool IGen3CacheFile.UsesStringEncryption => false;

        IPointerExpander IMccCacheFile.PointerExpander => PointerExpander;

        #endregion
    }

    [FixedSize(40960)]
    public class CacheHeader : IMccGen3Header
    {
        [Offset(8)]
        public virtual long FileSize { get; set; }

        [Offset(16)]
        public virtual Pointer64 IndexPointer { get; set; }

        [Offset(24)]
        public virtual int TagDataAddress { get; set; }

        [Offset(28)]
        public virtual int VirtualSize { get; set; }

        [Offset(288)]
        [NullTerminated(Length = 32)]
        public virtual string BuildString { get; set; }

        [Offset(348, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(336, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual int StringCount { get; set; }

        [Offset(352, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(340, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual int StringTableSize { get; set; }

        [Offset(356, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(344, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual Pointer StringTableIndexPointer { get; set; }

        [Offset(360, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(348, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual Pointer StringTablePointer { get; set; }

        [MinVersion((int)CacheType.MccHaloReachU3)]
        [Offset(352, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual int StringNamespaceCount { get; set; }

        [MinVersion((int)CacheType.MccHaloReachU3)]
        [Offset(356, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual Pointer StringNamespaceTablePointer { get; set; }

        [Offset(444, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(440, MinVersion = (int)CacheType.MccHaloReachU3)]
        [NullTerminated(Length = 256)]
        public virtual string ScenarioName { get; set; }

        [Offset(704, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(700, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual int FileCount { get; set; }

        [Offset(708, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(704, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual Pointer FileTablePointer { get; set; }

        [Offset(712, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(708, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual int FileTableSize { get; set; }

        [Offset(716, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(712, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual Pointer FileTableIndexPointer { get; set; }

        [Offset(760, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(752, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual long VirtualBaseAddress { get; set; }

        [Offset(776, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(768, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual PartitionTable64 PartitionTable { get; set; }

        [Offset(1204, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(1196, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(1220, MaxVersion = (int)CacheType.MccHaloReachU3)]
        [Offset(1212, MinVersion = (int)CacheType.MccHaloReachU3)]
        public virtual SectionTable SectionTable { get; set; }

        #region IGen3Header

        IPartitionTable IGen3Header.PartitionTable => PartitionTable;

        #endregion
    }

    [FixedSize(76)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        internal Dictionary<int, string> Filenames { get; }
        internal List<TagClass> Classes { get; }

        [Offset(0)]
        public int TagClassCount { get; set; }

        [Offset(8)]
        public Pointer64 TagClassDataPointer { get; set; }

        [Offset(16)]
        public int TagCount { get; set; }

        [Offset(24)]
        public Pointer64 TagDataPointer { get; set; }

        public TagIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new Dictionary<int, IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();

            Classes = new List<TagClass>();
            Filenames = new Dictionary<int, string>();
        }

        internal void ReadItems()
        {
            if (items.Any())
                throw new InvalidOperationException();

            using (var reader = cache.CreateReader(cache.MetadataTranslator, cache.PointerExpander))
            {
                reader.Seek(TagClassDataPointer.Address, SeekOrigin.Begin);
                Classes.AddRange(reader.ReadEnumerable<TagClass>(TagClassCount));

                reader.Seek(TagDataPointer.Address, SeekOrigin.Begin);
                for (var i = 0; i < TagCount; i++)
                {
                    //every Reach map has an empty tag
                    var item = reader.ReadObject(new IndexItem(cache, i));
                    if (item.ClassIndex < 0)
                        continue;

                    items.Add(i, item);

                    if (item.ClassCode != CacheFactory.ScenarioClass && CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadArray<int>(TagCount);

                reader.Seek(cache.Header.FileTablePointer.Address, SeekOrigin.Begin);
                using (var tempReader = reader.CreateVirtualReader())
                {
                    for (var i = 0; i < TagCount; i++)
                    {
                        if (indices[i] == -1)
                        {
                            Filenames.Add(i, null);
                            continue;
                        }

                        tempReader.Seek(indices[i], SeekOrigin.Begin);
                        Filenames.Add(i, tempReader.ReadNullTerminatedString());
                    }
                }
            }

            try
            {
                sysItems[CacheFactory.ScenarioClass] = items.Values.Single(i => i.ClassCode == CacheFactory.ScenarioClass && i.FullPath == cache.Header.ScenarioName);
            }
            catch
            {
                throw Exceptions.AmbiguousScenarioReference();
            }
        }

        public IndexItem GetGlobalTag(string classCode) => sysItems.GetValueOrDefault(classCode);

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.Values.GetEnumerator();
    }

    public class StringIndex : IStringIndex
    {
        private readonly CacheFile cache;
        private readonly string[] items;

        internal virtual StringIdTranslator Translator { get; }

        public StringIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new string[cache.Header.StringCount];
            Translator = new StringIdTranslator(Resources.MccHaloReachStrings, cache.Metadata.StringIds);
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
                reader.Seek(cache.Header.StringTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadArray<int>(cache.Header.StringCount);

                reader.Seek(cache.Header.StringTablePointer.Address, SeekOrigin.Begin);
                using (var tempReader = reader.CreateVirtualReader())
                {
                    for (var i = 0; i < cache.Header.StringCount; i++)
                    {
                        if (indices[i] < 0)
                            continue;

                        tempReader.Seek(indices[i], SeekOrigin.Begin);
                        items[i] = tempReader.ReadNullTerminatedString();
                    }
                }
            }
        }

        public int StringCount => items.Length;

        public string this[int id] => items[Translator.GetStringIndex(id)];

        public int GetStringId(string value) => Translator.GetStringId(Array.IndexOf(items, value));

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
    public class TagClass
    {
        [Offset(0)]
        public int ClassId { get; set; }

        [Offset(4)]
        [FixedLength(4)]
        public string ParentClassCode { get; set; }

        [Offset(8)]
        [FixedLength(4)]
        public string ParentClassCode2 { get; set; }

        [Offset(12)]
        public StringId ClassName { get; set; }

        private string classCode;
        public string ClassCode
        {
            get
            {
                if (classCode == null)
                {
                    var bits = BitConverter.GetBytes(ClassId);
                    Array.Reverse(bits);
                    classCode = Encoding.UTF8.GetString(bits);
                }

                return classCode;
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {ClassName.Value}");
    }

    [FixedSize(8)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

        private readonly object cacheLock = new object();
        private object metadataCache;

        public IndexItem(CacheFile cache, int index)
        {
            this.cache = cache;
            Id = index;
        }

        public int Id { get; }

        [Offset(0)]
        public short ClassIndex { get; set; }

        [Offset(2)]
        public short Unknown { get; set; }

        [Offset(4)]
        public Pointer MetaPointer { get; set; }

        public int ClassId => cache.TagIndex.Classes[ClassIndex].ClassId;

        public string ClassCode => cache.TagIndex.Classes[ClassIndex].ClassCode;

        public string ClassName => cache.TagIndex.Classes[ClassIndex].ClassName.Value;

        public string FileName => Utils.GetFileName(FullPath);

        public string FullPath => cache.TagIndex.Filenames[Id];

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
                        metadataCache = lazy = new Lazy<T>(() => ReadMetadataInternal<T>());
                }

                return lazy.Value;
            }
            else
                return ReadMetadataInternal<T>();
        }

        private T ReadMetadataInternal<T>()
        {
            using (var reader = cache.CreateReader(cache.MetadataTranslator, cache.PointerExpander))
            {
                reader.RegisterInstance<IIndexItem>(this);

                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                return (T)reader.ReadObject(typeof(T), (int)cache.CacheType);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FullPath}");
        }
    }
}
