using Adjutant.Blam.Common;
using Adjutant.Blam.Common.Gen3;
using Adjutant.Properties;
using Adjutant.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class CacheFile : IGen3CacheFile
    {
        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }
        public CacheMetadata Metadata { get; }

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }
        public LocaleIndex LocaleIndex { get; }

        public IAddressTranslator HeaderTranslator { get; }
        public IAddressTranslator MetadataTranslator { get; }

        public CacheFile(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFile(CacheArgs args)
        {
            if (!File.Exists(args.FileName))
                throw Exceptions.FileNotFound(args.FileName);

            FileName = args.FileName;
            ByteOrder = args.ByteOrder;
            BuildString = args.BuildString;
            CacheType = args.CacheType;
            Metadata = args.Metadata;

            if (args.CacheType == CacheType.Halo3Beta)
            {
                HeaderTranslator = new BetaHeaderAddressTranslator(this);
                MetadataTranslator = new BetaTagAddressTranslator(this);
            }
            else
            {
                HeaderTranslator = new SectionAddressTranslator(this, 0);
                MetadataTranslator = new TagAddressTranslator(this);
            }

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeader>((int)CacheType);

            //change IndexPointer to use MetadataTranslator instead of HeaderTranslator
            Header.IndexPointer = new Pointer(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();

                switch (CacheType)
                {
                    case CacheType.Halo3Beta:
                        LocaleIndex = new LocaleIndex(this, 488, 28, 11);
                        break;
                    case CacheType.Halo3Retail:
                        LocaleIndex = new LocaleIndex(this, 452, 68, 12);
                        break;
                    case CacheType.Halo3ODST:
                        LocaleIndex = new LocaleIndex(this, 508, 68, 12);
                        break;
                }
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("play")?.ReadMetadata<cache_file_resource_layout_table>();
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<cache_file_resource_gestalt>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<scenario>();
            });
        }

        public DependencyReader CreateReader(IAddressTranslator translator) => CacheFactory.CreateReader(this, translator);

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        IGen3Header IGen3CacheFile.Header => Header;
        ILocaleIndex IGen3CacheFile.LocaleIndex => LocaleIndex;
        bool IGen3CacheFile.UsesStringEncryption => false;

        #endregion
    }

    [FixedSize(2048, MaxVersion = (int)CacheType.Halo3Retail)]
    [FixedSize(12288, MinVersion = (int)CacheType.Halo3Retail)]
    public class CacheHeader : IGen3Header
    {
        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public Pointer IndexPointer { get; set; }

        [Offset(20)]
        public int TagDataAddress { get; set; }

        [Offset(24)]
        public int VirtualSize { get; set; }

        [Offset(284)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }

        [Offset(352, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(344, MinVersion = (int)CacheType.Halo3Retail)]
        public int StringCount { get; set; }

        [Offset(356, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(348, MinVersion = (int)CacheType.Halo3Retail)]
        public int StringTableSize { get; set; }

        [Offset(360, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(352, MinVersion = (int)CacheType.Halo3Retail)]
        public Pointer StringTableIndexPointer { get; set; }

        [Offset(364, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(356, MinVersion = (int)CacheType.Halo3Retail)]
        public Pointer StringTablePointer { get; set; }

        [Offset(440, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(432, MinVersion = (int)CacheType.Halo3Retail)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }

        [Offset(700, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(692, MinVersion = (int)CacheType.Halo3Retail)]
        public int FileCount { get; set; }

        [Offset(704, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(696, MinVersion = (int)CacheType.Halo3Retail)]
        public Pointer FileTablePointer { get; set; }

        [Offset(708, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(700, MinVersion = (int)CacheType.Halo3Retail)]
        public int FileTableSize { get; set; }

        [Offset(712, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(704, MinVersion = (int)CacheType.Halo3Retail)]
        public Pointer FileTableIndexPointer { get; set; }

        [Offset(752, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(744, MinVersion = (int)CacheType.Halo3Retail)]
        public int VirtualBaseAddress { get; set; }

        [Offset(752)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public PartitionTable PartitionTable { get; set; }

        [Offset(1132)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(1148)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public SectionTable SectionTable { get; set; }

        #region IGen3Header

        long IGen3Header.FileSize
        {
            get { return FileSize; }
            set { FileSize = (int)value; }
        }

        long IGen3Header.VirtualBaseAddress
        {
            get { return VirtualBaseAddress; }
            set { VirtualBaseAddress = (int)value; }
        }

        IPartitionTable IGen3Header.PartitionTable => PartitionTable;

        int IGen3Header.StringNamespaceCount
        {
            get { return default(int); }
            set { }
        }

        Pointer IGen3Header.StringNamespaceTablePointer
        {
            get { return default(Pointer); }
            set { }
        }

        #endregion
    }

    [FixedSize(32)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        internal Dictionary<int, string> Filenames { get; }
        internal List<TagClass> Classes { get; }

        [Offset(0)]
        public int TagClassCount { get; set; }

        [Offset(4)]
        public Pointer TagClassDataPointer { get; set; }

        [Offset(8)]
        public int TagCount { get; set; }

        [Offset(12)]
        public Pointer TagDataPointer { get; set; }

        [Offset(16)]
        public int TagInfoHeaderCount { get; set; }

        [Offset(20)]
        public Pointer TagInfoHeaderPointer { get; set; }

        [Offset(24)]
        public int TagInfoHeaderCount2 { get; set; }

        [Offset(28)]
        public Pointer TagInfoHeaderPointer2 { get; set; }

        public TagIndex(CacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            items = new Dictionary<int, IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();

            Classes = new List<TagClass>();
            Filenames = new Dictionary<int, string>();
        }

        internal void ReadItems()
        {
            if (items.Any())
                throw new InvalidOperationException();

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.Seek(TagClassDataPointer.Address, SeekOrigin.Begin);
                Classes.AddRange(reader.ReadEnumerable<TagClass>(TagClassCount));

                reader.Seek(TagDataPointer.Address, SeekOrigin.Begin);
                for (int i = 0; i < TagCount; i++)
                {
                    //every Halo3 map has an empty tag
                    var item = reader.ReadObject(new IndexItem(cache, i));
                    if (item.ClassIndex < 0) continue;

                    items.Add(i, item);

                    if (item.ClassCode != CacheFactory.ScenarioClass && CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                using (var tempReader = reader.CreateVirtualReader(cache.Header.FileTablePointer.Address))
                {
                    for (int i = 0; i < TagCount; i++)
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
            catch { throw Exceptions.AmbiguousScenarioReference(); }
        }

        public IndexItem GetGlobalTag(string classCode) => sysItems.ValueOrDefault(classCode);

        public IndexItem this[int index] => items[index];

        public IEnumerator<IndexItem> GetEnumerator() => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.Values.GetEnumerator();
    }

    public class StringIndex : IStringIndex
    {
        private readonly CacheFile cache;
        private readonly StringIdTranslator translator;
        private readonly string[] items;

        public StringIndex(CacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            items = new string[cache.Header.StringCount];
            translator = new StringIdTranslator(Resources.Halo3Strings, cache.Metadata.StringIds);
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
                reader.Seek(cache.Header.StringTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(cache.Header.StringCount).ToArray();

                using (var tempReader = reader.CreateVirtualReader(cache.Header.StringTablePointer.Address))
                {
                    for (int i = 0; i < cache.Header.StringCount; i++)
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

        public string this[int id] => items[translator.GetStringIndex(id)];

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
    public class TagClass
    {
        [Offset(0)]
        public int ClassId { get; set; }

        [Offset(0)]
        [FixedLength(4)]
        public string ClassCode { get; set; }

        [Offset(4)]
        [FixedLength(4)]
        public string ParentClassCode { get; set; }

        [Offset(8)]
        [FixedLength(4)]
        public string ParentClassCode2 { get; set; }

        [Offset(12)]
        public StringId ClassName { get; set; }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {ClassName.Value}");
        }
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
            var lazy = metadataCache as Lazy<T>;
            if (lazy != null)
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
            else return ReadMetadataInternal<T>();
        }

        private T ReadMetadataInternal<T>()
        {
            using (var reader = cache.CreateReader(cache.MetadataTranslator))
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
