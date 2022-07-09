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

namespace Reclaimer.Blam.MccHalo4
{
    public class CacheFileU4 : CacheFile
    {
        public override CacheHeader Header { get; }
        public override TagIndex TagIndex { get; }
        public override StringIndex StringIndex { get; }
        public override LocaleIndex LocaleIndex { get; }

        public override SectionAddressTranslator HeaderTranslator { get; }
        public override TagAddressTranslator MetadataTranslator { get; }

        public override PointerExpander PointerExpander { get; }

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
                StringIndex = new StringIndex(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();
                LocaleIndex = new LocaleIndex(this, 712, 80, 17);
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<Halo4.cache_file_resource_gestalt>();
                TagIndex.GetGlobalTag("play")?.ReadMetadata<Halo4.cache_file_resource_layout_table>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<Halo4.scenario>();
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
        internal override StringIdTranslator Translator { get; }
        
        public StringIndexU4(CacheFile cache)
            : base(cache)
        {
            Translator = new StringIdTranslator(cache, Resources.MccHalo4Strings, cache.Metadata.StringIds);
        }
    }
}
