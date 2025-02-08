using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.MccHaloReach
{
    public class CacheFileU8 : CacheFile
    {
        public CacheFileU8(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFileU8(CacheArgs args)
            : base(args.FileName, args.ByteOrder, args.BuildString, args.CacheType, args.Metadata)
        {
            HeaderTranslator = new SectionAddressTranslator(this, 0);
            MetadataTranslator = new TagAddressTranslator(this);
            PointerExpander = new PointerExpander(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeaderU8>((int)CacheType);

            //change IndexPointer to use MetadataTranslator instead of HeaderTranslator
            Header.IndexPointer = new Pointer64(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndexU8(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();

                LocaleIndex = args.CacheType >= CacheType.MccHaloReachU13
                    ? new LocaleIndex(this, 24, 80, 12)
                    : new LocaleIndex(this, 664, 80, 12);
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("play")?.ReadMetadata<HaloReach.CacheFileResourceLayoutTableTag>();
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<HaloReach.CacheFileResourceGestaltTag>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<HaloReach.ScenarioTag>();
            });
        }
    }

    [FixedSize(40960)]
    public class CacheHeaderU8 : CacheHeader
    {
        [Offset(8)]
        [StoreType(typeof(int))]
        public override long FileSize { get; set; }

        [Offset(16)]
        public override int TagDataAddress { get; set; }

        [Offset(20)]
        public override int VirtualSize { get; set; }

        [Offset(32)]
        public override int FileCount { get; set; }

        [Offset(36)]
        public override Pointer FileTablePointer { get; set; }

        [Offset(40)]
        public override int FileTableSize { get; set; }

        [Offset(44)]
        public override Pointer FileTableIndexPointer { get; set; }

        [Offset(48)]
        public override int StringCount { get; set; }

        [Offset(52)]
        public override Pointer StringTablePointer { get; set; }

        [Offset(56)]
        public override int StringTableSize { get; set; }

        [Offset(60)]
        public override Pointer StringTableIndexPointer { get; set; }

        [Offset(64)]
        public override int StringNamespaceCount { get; set; }

        [Offset(68)]
        public override Pointer StringNamespaceTablePointer { get; set; }

        [Offset(160)]
        [NullTerminated(Length = 32)]
        public override string BuildString { get; set; }

        [Offset(224)]
        [NullTerminated(Length = 256)]
        public override string ScenarioName { get; set; }

        [Offset(736)]
        public override long VirtualBaseAddress { get; set; }

        [Offset(744)]
        public override Pointer64 IndexPointer { get; set; }

        [Offset(768)]
        public override PartitionTable64 PartitionTable { get; set; }

        [Offset(1196, MaxVersion = (int)CacheType.MccHaloReachU10)]
        [Offset(1228, MinVersion = (int)CacheType.MccHaloReachU10)]
        public override SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(1212, MaxVersion = (int)CacheType.MccHaloReachU10)]
        [Offset(1244, MinVersion = (int)CacheType.MccHaloReachU10)]
        public override SectionTable SectionTable { get; set; }
    }

    public class StringIndexU8 : StringIndex
    {
        public StringIndexU8(CacheFileU8 cache)
            : base(cache)
        {
            Translator = new StringIdTranslator(cache, Resources.MccHaloReachStrings, cache.Metadata.StringIds);
        }
    }
}
