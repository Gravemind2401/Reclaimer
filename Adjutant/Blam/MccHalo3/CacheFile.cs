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

namespace Adjutant.Blam.MccHalo3
{
    public class CacheFile : ICacheFile
    {
        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }

        public HeaderAddressTranslator HeaderTranslator { get; }
        public TagAddressTranslator MetadataTranslator { get; }

        public PointerExpander PointerExpander { get; }

        public CacheFile(string fileName) : this(CacheDetail.FromFile(fileName)) { }

        internal CacheFile(CacheDetail detail)
        {
            if (!File.Exists(detail.FileName))
                throw Exceptions.FileNotFound(detail.FileName);

            FileName = detail.FileName;
            ByteOrder = detail.ByteOrder;
            BuildString = detail.BuildString;
            CacheType = detail.CacheType;

            HeaderTranslator = new HeaderAddressTranslator(this);
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
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("play")?.ReadMetadata<Halo3.cache_file_resource_layout_table>();
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<Halo3.cache_file_resource_gestalt>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<Halo3.scenario>();
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

        #endregion
    }

    [FixedSize(12288)]
    public class CacheHeader
    {
        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public Pointer64 IndexPointer { get; set; }

        [Offset(24)]
        public int TagDataAddress { get; set; }

        [Offset(288)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }

        [Offset(348)]
        public int StringCount { get; set; }

        [Offset(352)]
        public int StringTableSize { get; set; }

        [Offset(356)]
        public int StringTableIndexAddress { get; set; }

        [Offset(360)]
        public int StringTableAddress { get; set; }

        [Offset(444)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }

        [Offset(704)]
        public int FileCount { get; set; }

        [Offset(708)]
        public int FileTableAddress { get; set; }

        [Offset(712)]
        public int FileTableSize { get; set; }

        [Offset(716)]
        public int FileTableIndexAddress { get; set; }

        [Offset(760)]
        public long VirtualBaseAddress { get; set; }

        [Offset(1208)]
        public int ResourceModifier { get; set; }

        [Offset(1212)]
        public int TagModifier { get; set; }

        [Offset(1216)]
        public int LocaleModifier { get; set; }

        [Offset(1232)]
        public int ResourceDataSize { get; set; }
    }

    [FixedSize(72)]
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

            using (var reader = cache.CreateReader(cache.MetadataTranslator, cache.PointerExpander))
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

                    if (CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                reader.Seek(cache.Header.FileTableIndexAddress, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                using (var tempReader = reader.CreateVirtualReader(cache.Header.FileTableAddress))
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

            var xml = Adjutant.Properties.Resources.MccHalo3Strings;
            switch (cache.CacheType)
            {
                case CacheType.MccHalo3:
                    translator = new StringIdTranslator(xml, "U0");
                    break;

                case CacheType.MccHalo3ODST:
                    translator = new StringIdTranslator(xml, "ODST U0");
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
                reader.Seek(cache.Header.StringTableIndexAddress, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(cache.Header.StringCount).ToArray();

                using (var tempReader = reader.CreateVirtualReader(cache.Header.StringTableAddress))
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
            using (var reader = cache.CreateReader(cache.MetadataTranslator, cache.PointerExpander))
            {
                reader.RegisterInstance<IIndexItem>(this);

                // hack to redirect any play tag reads to the start of the zone tag when there is no play tag
                if (ClassCode == "play" && MetaPointer.Value == 0)
                    reader.Seek(cache.TagIndex.GetGlobalTag("zone").MetaPointer.Address + 28, SeekOrigin.Begin);
                else
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
