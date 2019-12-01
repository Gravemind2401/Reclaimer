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

namespace Adjutant.Blam.Halo4
{
    public class CacheFile : ICacheFile
    {
        public const string FileNamesKey = "LetsAllPlayNice!";
        public const string StringsKey = "ILikeSafeStrings";
        public const string LocalesKey = "BungieHaloReach!";
        public const string NetworkKey = "SneakerNetReigns";

        public int HeaderSize => 122880;

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
            Header.IndexPointer = new Pointer(Header.IndexPointer.Value, MetadataTranslator);

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

    [FixedSize(122880)]
    public class CacheHeader
    {
        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public Pointer IndexPointer { get; set; }

        [Offset(284)]
        [NullTerminated(Length = 32)]
        public string BuildString { get; set; }

        [Offset(344)]
        public int StringCount { get; set; }

        [Offset(348)]
        public int StringTableSize { get; set; }

        [Offset(352)]
        public Pointer StringTableIndexPointer { get; set; }

        [Offset(356)]
        public Pointer StringTablePointer { get; set; }

        [Offset(432)]
        [NullTerminated(Length = 256)]
        public string ScenarioName { get; set; }

        [Offset(692)]
        public int FileCount { get; set; }

        [Offset(696)]
        public Pointer FileTablePointer { get; set; }

        [Offset(700)]
        public int FileTableSize { get; set; }

        [Offset(704)]
        public Pointer FileTableIndexPointer { get; set; }

        [Offset(760)]
        public int VirtualBaseAddress { get; set; }

        [Offset(1152)]
        public int DataTableAddress { get; set; }

        [Offset(1160)]
        public int LocaleModifier { get; set; }

        [Offset(1176)]
        public int DataTableSize { get; set; }

        [Offset(1180)]
        public int MetadataAddress { get; set; }
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
                    var item = reader.ReadObject(new IndexItem(cache, i));
                    if (item.ClassIndex < 0) continue;

                    items.Add(i, item);

                    if (CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                var play = sysItems["play"];
                if (play.MetaPointer.Value == 0)
                    play.MetaPointer = new Pointer(sysItems["zone"].MetaPointer.Value + 28, cache.MetadataTranslator);

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                reader.Seek(cache.Header.FileTablePointer.Address, SeekOrigin.Begin);
                var decrypted = reader.ReadAesBytes(cache.Header.FileTableSize, CacheFile.FileNamesKey);
                using (var ms = new MemoryStream(decrypted))
                using (var tempReader = new EndianReader(ms))
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
                var decrypted = reader.ReadAesBytes(cache.Header.StringTableSize, CacheFile.StringsKey);
                using (var ms = new MemoryStream(decrypted))
                using (var tempReader = new EndianReader(ms))
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
                try
                {
                    if (cache.CacheType == CacheType.Halo4Beta)
                    {
                        if (id > 3662803) return items[id - 3662803];
                        else if (id > 260655) return items[id - 260655];
                        else if (id > 1488) return items[id + 5937];
                    }
                    else if (cache.CacheType == CacheType.Halo4Retail)
                    {
                        if (cache.BuildString == "16531.12.07.05.0200.main")
                        {
                            if (id > 1568) return items[id + 6520];
                        }
                        else
                        {
                            if (id > 7331943) return items[id - 7331943];
                            else if (id > 522703) return items[id - 522703];
                            else if (id > 1584) return items[id + 6796];
                        }
                    }

                    return items[id];
                }
                catch { return "#error"; }
            }
        }

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
    public class TagClass
    {
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
