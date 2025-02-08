using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class CacheFileResourceGestaltTag
    {
        [Offset(88)]
        public BlockCollection<ResourceEntryBlock> ResourceEntries { get; set; }

        [Offset(316, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(340, MinVersion = (int)CacheType.Halo4Retail)]
        public int FixupDataSize { get; set; }

        [Offset(328, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(352, MinVersion = (int)CacheType.Halo4Retail)]
        public Pointer FixupDataPointer { get; set; }
    }

    [FixedSize(68)]
    public class ResourceEntryBlock
    {
        [Offset(0)]
        public TagReference OwnerReference { get; set; }

        [Offset(20)]
        public int FixupSize { get; set; }

        [Offset(26)]
        public short SegmentIndex { get; set; }

        [Offset(32)]
        public BlockCollection<ResourceFixupBlock> ResourceFixups { get; set; }

        [Offset(44)]
        public BlockCollection<ResourceDefinitionFixupBlock> ResourceDefinitionFixups { get; set; }

        [Offset(56)]
        public BlockCollection<int> FixupOffsets { get; set; }
    }

    [FixedSize(8)]
    public class ResourceFixupBlock
    {
        [Offset(0)]
        public int Unknown { get; set; }

        [Offset(4)]
        public int Offset { get; set; } //mask 0x0FFFFFFF (4 bit ?, 28 bit offset)

        public int MaskedOffset => Offset & 0x0FFFFFFF;
    }

    [FixedSize(8)]
    public class ResourceDefinitionFixupBlock
    {
        [Offset(0)]
        public int Offset { get; set; }

        [Offset(4)]
        public int Unknown { get; set; }
    }
}
