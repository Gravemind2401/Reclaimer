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

namespace Adjutant.Blam.MccHaloReach
{
    public class CacheFile : ICacheFile
    {
        public int HeaderSize => 40960;

        public string FileName { get; }
        public string BuildString => Header?.BuildString;
        public CacheType CacheType => CacheFactory.GetCacheTypeByBuild(BuildString);

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }

        public HeaderAddressTranslator HeaderTranslator { get; }
        public TagAddressTranslator MetadataTranslator { get; }

        public CacheFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            var version = (int)CacheFactory.GetCacheTypeByFile(fileName);

            FileName = fileName;
            HeaderTranslator = new HeaderAddressTranslator(this);
            MetadataTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeader>(version);

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
        }

        public DependencyReader CreateReader(IAddressTranslator translator) => CacheFactory.CreateReader(this, translator);

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        #endregion
    }

    [FixedSize(40960)]
    public class CacheHeader
    {
        [Offset(8)]
        public long FileSize { get; set; }

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
        public Pointer StringTableIndexPointer { get; set; }

        [Offset(360)]
        public Pointer StringTablePointer { get; set; }

        [Offset(444)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }

        [Offset(704)]
        public int FileCount { get; set; }

        [Offset(708)]
        public Pointer FileTablePointer { get; set; }

        [Offset(712)]
        public int FileTableSize { get; set; }

        [Offset(716)]
        public Pointer FileTableIndexPointer { get; set; }

        [Offset(760)]
        public long VirtualBaseAddress { get; set; }

        [Offset(1208)]
        public int ResourceModifier { get; set; }

        [Offset(1212)]
        public int TagModifier { get; set; }

        [Offset(1216)]
        public int LocaleModifier { get; set; }

        [Offset(1224)]
        public long DataTableAddress { get; set; }

        [Offset(1244)]
        public int DataTableSize { get; set; }
    }

    [FixedSize(48)]
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

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.Seek(TagClassDataPointer.Address, SeekOrigin.Begin);
                Classes.AddRange(reader.ReadEnumerable<TagClass>(TagClassCount));

                reader.Seek(TagDataPointer.Address, SeekOrigin.Begin);
                for (int i = 0; i < TagCount; i++)
                {
                    //every Reach map has an empty tag
                    var item = reader.ReadObject(new IndexItem(cache, i));
                    if (item.ClassIndex < 0) continue;

                    items.Add(i, item);

                    if (CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                reader.Seek(cache.Header.FileTablePointer.Address, SeekOrigin.Begin);
                using (var tempReader = reader.CreateVirtualReader())
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
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            items = new string[cache.Header.StringCount];
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
                reader.Seek(cache.Header.StringTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(cache.Header.StringCount).ToArray();

                reader.Seek(cache.Header.StringTablePointer.Address, SeekOrigin.Begin);
                using (var tempReader = reader.CreateVirtualReader())
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

        public string this[int id]
        {
            get
            {
                return items[id];
            }
        }

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
    public class TagClass
    {
        [Offset(0)]
        [ByteOrder(ByteOrder.BigEndian)]
        public int ClassId { get; set; }

        [Offset(4)]
        [FixedLength(4)]
        public string ParentClassCode { get; set; }

        [Offset(8)]
        [FixedLength(4)]
        public string ParentClassCode2 { get; set; }

        [Offset(12)]
        public StringId ClassName { get; set; }

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId));

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

        public string ClassCode => cache.TagIndex.Classes[ClassIndex].ClassCode;

        public string ClassName => cache.TagIndex.Classes[ClassIndex].ClassName.Value;

        public string FileName => Utils.GetFileName(FullPath);

        public string FullPath => cache.TagIndex.Filenames[Id];

        public T ReadMetadata<T>()
        {
            if (metadataCache?.GetType() == typeof(T))
                return (T)metadataCache;

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.RegisterInstance<IIndexItem>(this);

                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                var result = (T)reader.ReadObject(typeof(T), (int)cache.CacheType);

                if (CacheFactory.SystemClasses.Contains(ClassCode))
                    metadataCache = result;

                return result;
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FullPath}");
        }
    }
}
