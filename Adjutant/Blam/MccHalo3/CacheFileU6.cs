using Adjutant.Blam.Common;
using Adjutant.Blam.Common.Gen3;
using Adjutant.Properties;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.MccHalo3
{
    public class CacheFileU6 : CacheFile
    {
        public override CacheHeader Header { get; }
        public override TagIndex TagIndex { get; }
        public override StringIndex StringIndex { get; }
        public override LocaleIndex LocaleIndex { get; }

        public override SectionAddressTranslator HeaderTranslator { get; }
        public override TagAddressTranslator MetadataTranslator { get; }

        public override PointerExpander PointerExpander { get; }

        public CacheFileU6(string fileName) : this(CacheArgs.FromFile(fileName)) { }

        internal CacheFileU6(CacheArgs args)
            : base(args.FileName, args.ByteOrder, args.BuildString, args.CacheType, args.Metadata)
        {
            HeaderTranslator = new SectionAddressTranslator(this, 0);
            MetadataTranslator = new TagAddressTranslator(this);
            PointerExpander = new PointerExpander(this);

            using (var reader = CreateReader(HeaderTranslator))
                Header = reader.ReadObject<CacheHeaderU6>((int)CacheType);

            //change IndexPointer to use MetadataTranslator instead of HeaderTranslator
            Header.IndexPointer = new Pointer64(Header.IndexPointer.Value, MetadataTranslator);

            using (var reader = CreateReader(MetadataTranslator))
            {
                reader.Seek(Header.IndexPointer.Address, SeekOrigin.Begin);
                TagIndex = reader.ReadObject(new TagIndex(this));
                StringIndex = new StringIndexU6(this);

                TagIndex.ReadItems();
                StringIndex.ReadItems();

                switch (CacheType)
                {
                    case CacheType.MccHalo3U6:
                        LocaleIndex = new LocaleIndex(this, 464, 80, 12);
                        break;
                    case CacheType.MccHalo3ODSTU3:
                        LocaleIndex = new LocaleIndex(this, 520, 80, 12);
                        break;
                }
            }

            Task.Factory.StartNew(() =>
            {
                TagIndex.GetGlobalTag("play")?.ReadMetadata<Halo3.cache_file_resource_layout_table>();
                TagIndex.GetGlobalTag("zone")?.ReadMetadata<Halo3.cache_file_resource_gestalt>();
                TagIndex.GetGlobalTag("scnr")?.ReadMetadata<Halo3.scenario>();
            });
        }
    }

    [FixedSize(16384)]
    public class CacheHeaderU6 : CacheHeader, IMccGen3Header
    {
        [Offset(8)]
        [StoreType(typeof(int))]
        public override long FileSize { get; set; }

        [Offset(16)]
        public override int TagDataAddress { get; set; }

        [Offset(20)]
        public override int VirtualSize { get; set; }

        [Offset(0x20)]
        public override int FileCount { get; set; }

        [Offset(0x24)]
        public override Pointer FileTablePointer { get; set; }

        [Offset(0x28)]
        public override int FileTableSize { get; set; }

        [Offset(0x2C)]
        public override Pointer FileTableIndexPointer { get; set; }

        [Offset(0x30)]
        public override int StringCount { get; set; }

        [Offset(0x34)]
        public override Pointer StringTablePointer { get; set; }

        [Offset(0x38)]
        public override int StringTableSize { get; set; }

        [Offset(0x3C)]
        public override Pointer StringTableIndexPointer { get; set; }

        [Offset(0x40)]
        public int StringNamespaceCount { get; set; }

        [Offset(0x44)]
        public Pointer StringNamespaceTablePointer { get; set; }

        [Offset(0xA0)]
        [NullTerminated(Length = 32)]
        public override string BuildString { get; set; }

        [Offset(0xE0)]
        [NullTerminated(Length = 256)]
        public override string ScenarioName { get; set; }

        [Offset(0x2E0)]
        public override long VirtualBaseAddress { get; set; }

        [Offset(0x2E8)]
        public override Pointer64 IndexPointer { get; set; }

        [Offset(0x300)]
        public override PartitionTable64 PartitionTable { get; set; }

        [Offset(0x4AC)]
        public override SectionOffsetTable SectionOffsetTable { get; set; }

        [Offset(0x4BC)]
        public override SectionTable SectionTable { get; set; }
    }

    public class StringIndexU6 : StringIndex
    {
        internal override StringIdTranslator Translator { get; }

        public StringIndexU6(CacheFileU6 cache)
            : base(cache)
        {
            Translator = new StringIdTranslator(cache, Resources.MccHalo3Strings, cache.Metadata.StringIds);
        }
    }
}
