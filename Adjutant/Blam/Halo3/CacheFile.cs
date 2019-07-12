using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class CacheFile : ICacheFile
    {
        public string FileName { get; }
        public string BuildString => Header.BuildString;
        public CacheType CacheType => CacheFactory.GetCacheTypeByBuild(BuildString);

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }

        public scenario Scenario { get; }

        private cache_file_resource_gestalt resourceGestalt;
        public cache_file_resource_gestalt ResourceGestalt
        {
            get
            {
                if (resourceGestalt == null)
                    resourceGestalt = TagIndex.FirstOrDefault(t => t.ClassCode == "zone")?.ReadMetadata<cache_file_resource_gestalt>();
                return resourceGestalt;
            }
        }

        private cache_file_resource_layout_table resourceLayoutTable;
        public cache_file_resource_layout_table ResourceLayoutTable
        {
            get
            {
                if (resourceLayoutTable == null)
                    resourceLayoutTable = TagIndex.FirstOrDefault(t => t.ClassCode == "play")?.ReadMetadata<cache_file_resource_layout_table>();
                return resourceLayoutTable;
            }
        }

        public HeaderAddressTranslator HeaderTranslator { get; }
        public TagAddressTranslator MetadataTranslator { get; }

        public CacheFile(string fileName)
        {
            var version = (int)CacheFactory.GetCacheTypeByFile(fileName);

            FileName = fileName;
            HeaderTranslator = new HeaderAddressTranslator(this);
            MetadataTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeader>(version);

            Header.IndexPointer = new Pointer(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();
            }

            Scenario = TagIndex.FirstOrDefault(t => t.ClassCode == "scnr")?.ReadMetadata<scenario>();
        }

        public DependencyReader CreateReader(IAddressTranslator translator)
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder.LittleEndian);

            var header = reader.PeekInt32();
            if (header == CacheFactory.BigHeader)
                reader.ByteOrder = ByteOrder.BigEndian;
            else if (header != CacheFactory.LittleHeader)
                throw Exceptions.NotAValidMapFile(Path.GetFileName(FileName));

            reader.RegisterInstance<CacheFile>(this);
            reader.RegisterInstance<ICacheFile>(this);
            reader.RegisterInstance<IAddressTranslator>(translator);
            reader.RegisterType<Matrix4x4>(() => new Matrix4x4
            {
                M11 = reader.ReadSingle(),
                M12 = reader.ReadSingle(),
                M13 = reader.ReadSingle(),

                M21 = reader.ReadSingle(),
                M22 = reader.ReadSingle(),
                M23 = reader.ReadSingle(),

                M31 = reader.ReadSingle(),
                M32 = reader.ReadSingle(),
                M33 = reader.ReadSingle(),

                M41 = reader.ReadSingle(),
                M42 = reader.ReadSingle(),
                M43 = reader.ReadSingle(),
            });

            return reader;
        }

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        #endregion
    }

    [FixedSize(2048, MaxVersion = (int)CacheType.Halo3Retail)]
    [FixedSize(12288, MinVersion = (int)CacheType.Halo3Retail)]
    public class CacheHeader
    {
        [Offset(8)]
        public int FileSize { get; set; }

        [Offset(16)]
        public Pointer IndexPointer { get; set; }

        [Offset(20)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int MetadataAddress { get; set; }

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

        [Offset(1136)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public int DataTableAddress { get; set; }

        [Offset(1144)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public int LocaleModifier { get; set; }

        [Offset(1160)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public int DataTableSize { get; set; }
    }

    [FixedSize(32)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;

        internal Dictionary<int, string> Filenames { get; }
        internal List<TagClass> Classes { get; }

        public int Magic => 0;

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
                    if (item.ClassIndex >= 0) items.Add(i, item);
                }

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                for (int i = 0; i < TagCount; i++)
                {
                    if (indices[i] == -1)
                    {
                        Filenames.Add(i, null);
                        continue;
                    }

                    reader.Seek(cache.Header.FileTablePointer.Address + indices[i], SeekOrigin.Begin);
                    Filenames.Add(i, reader.ReadNullTerminatedString());
                }
            }
        }

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

                using (var reader2 = reader.CreateVirtualReader(cache.Header.StringTablePointer.Address))
                {
                    for (int i = 0; i < cache.Header.StringCount; i++)
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

        public string this[int id]
        {
            get
            {
                if (cache.CacheType == CacheType.Halo3Beta)
                {
                    if (id > 64307) return items[id - 64307];
                    else if (id > 1229) return items[id + 1173];
                }
                else if (cache.CacheType == CacheType.Halo3Retail)
                {
                    if (id > 262143) return items[id - 259153];
                    else if (id > 64329) return items[id - 64329];
                    else if (id > 1208) return items[id + 1882];
                }
                else if (cache.CacheType == CacheType.Halo3ODST)
                {
                    if (id > 258846) return items[id - 258846];
                    else if (id > 64231) return items[id - 64231];
                    else if (id > 1304) return items[id + 2098];
                }

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
    }

    [FixedSize(8)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

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

        public string FileName => cache.TagIndex.Filenames[Id];

        public T ReadMetadata<T>()
        {
            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.RegisterInstance<IndexItem>(this);

                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                return (T)reader.ReadObject(typeof(T), (int)cache.CacheType);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FileName}");
        }
    }
}
