﻿using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;
using System.IO;

namespace Reclaimer.Blam.Halo4
{
    public class CacheFile : IGen3CacheFile
    {
        public const string FileNamesKey = "LetsAllPlayNice!";
        public const string StringsKey = "ILikeSafeStrings";
        public const string LocalesKey = "BungieHaloReach!";
        public const string NetworkKey = "SneakerNetReigns";

        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public string BuildString { get; }
        public CacheType CacheType { get; }
        public CacheMetadata Metadata { get; }

        public CacheHeader Header { get; }
        public TagIndex TagIndex { get; }
        public StringIndex StringIndex { get; }
        public LocaleIndex LocaleIndex { get; }

        public SectionAddressTranslator HeaderTranslator { get; }
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

            HeaderTranslator = new SectionAddressTranslator(this, 0);
            MetadataTranslator = new TagAddressTranslator(this);

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
                    case CacheType.Halo4Beta:
                        LocaleIndex = new LocaleIndex(this, 696, 68, 12);
                        break;
                    case CacheType.Halo4Retail:
                        LocaleIndex = new LocaleIndex(this, 700, 68, 17);
                        break;
                }
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<CacheFileResourceGestaltTag>();
                TagIndex.GetGlobalTag("play")?.ReadMetadata<CacheFileResourceLayoutTableTag>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<ScenarioTag>();
            });
        }

        public DependencyReader CreateReader(IAddressTranslator translator) => CacheFactory.CreateReader(this, translator);

        #region ICacheFile

        ITagIndex<IIndexItem> ICacheFile.TagIndex => TagIndex;
        IStringIndex ICacheFile.StringIndex => StringIndex;
        IAddressTranslator ICacheFile.DefaultAddressTranslator => MetadataTranslator;

        IGen3Header IGen3CacheFile.Header => Header;
        ILocaleIndex IGen3CacheFile.LocaleIndex => LocaleIndex;
        bool IGen3CacheFile.UsesStringEncryption => true;

        #endregion
    }

    [FixedSize(122880)]
    public class CacheHeader : IGen4Header
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

        [Offset(708)]
        public int UnknownTableSize { get; set; }

        [Offset(712)]
        public Pointer UnknownTablePointer { get; set; }

        [Offset(760)]
        public int VirtualBaseAddress { get; set; }

        [Offset(768)]
        public PartitionTable PartitionTable { get; set; }

        [Offset(1148)]
        public SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(1164)]
        public SectionTable SectionTable { get; set; }

        #region IGen3Header

        long IGen3Header.FileSize
        {
            get => FileSize;
            set => FileSize = (int)value;
        }

        long IGen3Header.VirtualBaseAddress
        {
            get => VirtualBaseAddress;
            set => VirtualBaseAddress = (int)value;
        }

        IPartitionTable IGen3Header.PartitionTable => PartitionTable;

        #endregion
    }

    [FixedSize(32)]
    public class TagIndex : ITagIndex<IndexItem>
    {
        private readonly CacheFile cache;
        private readonly Dictionary<int, IndexItem> items;
        private readonly Dictionary<string, IndexItem> sysItems;

        internal Dictionary<int, string> TagNames { get; }
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
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            items = new Dictionary<int, IndexItem>();
            sysItems = new Dictionary<string, IndexItem>();

            Classes = new List<TagClass>();
            TagNames = new Dictionary<int, string>();
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
                for (var i = 0; i < TagCount; i++)
                {
                    var item = reader.ReadObject(new IndexItem(cache, i));
                    if (item.ClassIndex < 0)
                        continue;

                    items.Add(i, item);

                    if (item.ClassCode != CacheFactory.ScenarioClass && CacheFactory.SystemClasses.Contains(item.ClassCode))
                        sysItems.Add(item.ClassCode, item);
                }

                // hack to redirect any play tag requests to the start of the zone tag when there is no play tag
                var play = sysItems["play"];
                if (play.MetaPointer.Value == 0)
                    play.MetaPointer = new Pointer(sysItems["zone"].MetaPointer.Value + 28, play.MetaPointer);

                reader.Seek(cache.Header.FileTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadArray<int>(TagCount);

                reader.Seek(cache.Header.FileTablePointer.Address, SeekOrigin.Begin);
                var decrypted = reader.ReadAesBytes(cache.Header.FileTableSize, CacheFile.FileNamesKey);
                using (var ms = new MemoryStream(decrypted))
                using (var tempReader = new EndianReader(ms))
                {
                    for (var i = 0; i < TagCount; i++)
                    {
                        if (indices[i] == -1)
                        {
                            TagNames.Add(i, null);
                            continue;
                        }

                        tempReader.Seek(indices[i], SeekOrigin.Begin);
                        TagNames.Add(i, tempReader.ReadNullTerminatedString());
                    }
                }
            }

            try
            {
                sysItems[CacheFactory.ScenarioClass] = items.Values.Single(i => i.ClassCode == CacheFactory.ScenarioClass && i.TagName == cache.Header.ScenarioName);
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

    public class StringIndex : StringIndexBase
    {
        private readonly CacheFile cache;
        private readonly StringIdTranslator translator;

        public StringIndex(CacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Items = new string[cache.Header.StringCount];
            translator = new StringIdTranslator(Resources.Halo4Strings, cache.Metadata.StringIds);
        }

        internal void ReadItems()
        {
            using (var reader = cache.CreateReader(cache.HeaderTranslator))
            {
                reader.Seek(cache.Header.StringTableIndexPointer.Address, SeekOrigin.Begin);
                var indices = reader.ReadArray<int>(cache.Header.StringCount);

                reader.Seek(cache.Header.StringTablePointer.Address, SeekOrigin.Begin);
                var decrypted = reader.ReadAesBytes(cache.Header.StringTableSize, CacheFile.StringsKey);
                using (var ms = new MemoryStream(decrypted))
                using (var tempReader = new EndianReader(ms))
                {
                    for (var i = 0; i < cache.Header.StringCount; i++)
                    {
                        if (indices[i] < 0)
                            continue;

                        tempReader.Seek(indices[i], SeekOrigin.Begin);
                        Items[i] = tempReader.ReadNullTerminatedString();
                    }
                }
            }
        }

        protected override int GetStringIndex(int id) => translator.GetStringIndex(id);

        public override int GetStringId(string value) => translator.GetStringId(Array.IndexOf(Items, value));
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
                        metadataCache = lazy = new Lazy<T>(ReadMetadataInternal);
                }

                return lazy.Value;
            }
            else
                return ReadMetadataInternal();

            T ReadMetadataInternal()
            {
                using (var reader = cache.CreateReader(cache.MetadataTranslator))
                {
                    reader.RegisterInstance<IIndexItem>(this);
                    reader.Seek(MetaPointer.Address, SeekOrigin.Begin);
                    return reader.ReadObject<T>((int)cache.CacheType);
                }
            }
        }

        public override string ToString() => Utils.CurrentCulture($"[{ClassCode}] {TagName}");
    }
}
