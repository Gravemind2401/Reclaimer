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

namespace Adjutant.Blam.Halo2
{
    public class CacheFile : ICacheFile
    {
        public const string MainMenuMap = "mainmenu.map";
        public const string SharedMap = "shared.map";
        public const string SinglePlayerSharedMap = "single_player_shared.map";

        public string FileName { get; }
        public string BuildString => Header.BuildString;
        public CacheType CacheType => CacheFactory.GetCacheTypeByBuild(BuildString);

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }

        public scenario Scenario { get; }

        public HeaderAddressTranslator HeaderTranslator { get; }
        public TagAddressTranslator MetadataTranslator { get; }

        public CacheFile(string fileName)
        {
            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            FileName = fileName;
            HeaderTranslator = new HeaderAddressTranslator(this);
            MetadataTranslator = new TagAddressTranslator(this);

            using (var reader = CreateReader(HeaderTranslator))
            {
                reader.DynamicReadEnabled = false;
                Header = reader.ReadObject<CacheHeader>();
                reader.Seek(Header.IndexAddress, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();
            }

            Scenario = TagIndex.FirstOrDefault(t => t.ClassCode == "scnr")?.ReadMetadata<scenario>();
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
        public int MetadataAddress { get; set; }

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

        public int HeaderSize => 32;

        internal Dictionary<int, string> Filenames { get; }

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
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            items = new Dictionary<int, IndexItem>();
            Filenames = new Dictionary<int, string>();
        }

        internal void ReadItems()
        {
            if (items.Any())
                throw new InvalidOperationException();

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.Seek(TagDataAddress.Address, SeekOrigin.Begin);
                for (int i = 0; i < TagCount; i++)
                {
                    //Halo2Vista multiplayer maps have empty tags in them
                    var item = reader.ReadObject(new IndexItem(cache));
                    if (item.Id >= 0) items.Add(i, item);
                }

                reader.Seek(cache.Header.FileTableIndexOffset, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<int>(TagCount).ToArray();

                for (int i = 0; i < TagCount; i++)
                {
                    reader.Seek(cache.Header.FileTableAddress + indices[i], SeekOrigin.Begin);
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
                var indices = new int[cache.Header.StringCount];
                reader.Seek(cache.Header.StringTableIndexAddress, SeekOrigin.Begin);
                for (int i = 0; i < cache.Header.StringCount; i++)
                    indices[i] = reader.ReadInt32();

                using (var reader2 = reader.CreateVirtualReader(cache.Header.StringTableAddress))
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

        public string this[int id] => items[id];

        public IEnumerator<string> GetEnumerator() => items.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }

    [FixedSize(16)]
    public class IndexItem : IIndexItem
    {
        private readonly CacheFile cache;
        ICacheFile IIndexItem.CacheFile => cache;

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

        public string ClassCode => Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId).Reverse().ToArray());

        public string ClassName
        {
            get
            {
                if (CacheFactory.Halo2Classes.ContainsKey(ClassCode))
                    return CacheFactory.Halo2Classes[ClassCode];

                return ClassCode;
            }
        }

        public string FullPath => cache.TagIndex.Filenames[Id];

        public T ReadMetadata<T>()
        {
            if (typeof(T).Equals(typeof(scenario_structure_bsp)))
            {
                var translator = new BSPAddressTranslator(cache, Id);
                using (var reader = cache.CreateReader(translator))
                {
                    reader.RegisterInstance<IIndexItem>(this);
                    reader.Seek(translator.TagAddress, SeekOrigin.Begin);
                    return (T)(object)reader.ReadObject<scenario_structure_bsp>(cache.Header.Version);
                }
            }

            using (var reader = cache.CreateReader(cache.MetadataTranslator))
            {
                reader.RegisterInstance<IIndexItem>(this);
                reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                return (T)reader.ReadObject(typeof(T), cache.Header.Version);
            }
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{ClassCode}] {FullPath}");
        }
    }
}
