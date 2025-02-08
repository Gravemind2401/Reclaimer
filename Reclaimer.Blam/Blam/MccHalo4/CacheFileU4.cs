using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.MccHalo4
{
    public class CacheFileU4 : CacheFile
    {
        public CacheFileU4(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFileU4(CacheArgs args)
            : base(args.FileName, args.ByteOrder, args.BuildString, args.CacheType, args.Metadata)
        {
            HeaderTranslator = new SectionAddressTranslator(this, 0);
            MetadataTranslator = new TagAddressTranslator(this);
            PointerExpander = new PointerExpander(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeaderU4>((int)CacheType);

            //change IndexPointer to use MetadataTranslator instead of HeaderTranslator
            Header.IndexPointer = new Pointer64(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndexU4(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();

                var offset = args.CacheType >= CacheType.MccHalo4U6 ? 24 : 712;
                LocaleIndex = new LocaleIndex(this, offset, 80, 17);
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<Halo4.CacheFileResourceGestaltTag>();
                TagIndex.GetGlobalTag("play")?.ReadMetadata<Halo4.CacheFileResourceLayoutTableTag>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<Halo4.ScenarioTag>();
            });
        }
    }

    [FixedSize(122880)]
    public class CacheHeaderU4 : CacheHeader, IMccGen4Header
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
        public int StringNamespaceCount { get; set; }

        [Offset(68)]
        public Pointer StringNamespaceTablePointer { get; set; }

        [Offset(152)]
        [NullTerminated(Length = 32)]
        public override string BuildString { get; set; }

        [Offset(216)]
        [NullTerminated(Length = 256)]
        public override string ScenarioName { get; set; }

        [Offset(728)]
        public override long VirtualBaseAddress { get; set; }

        [Offset(736)]
        public override Pointer64 IndexPointer { get; set; }

        [Offset(760)]
        public override PartitionTable64 PartitionTable { get; set; }

        [Offset(1220)]
        public override SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(1236)]
        public override SectionTable SectionTable { get; set; }
    }

    public class StringIndexU4 : StringIndex
    {
        public StringIndexU4(CacheFile cache)
            : base(cache)
        {
            Translator = new StringIdTranslator(cache, Resources.MccHalo4Strings, cache.Metadata.StringIds);
        }
    }
}
