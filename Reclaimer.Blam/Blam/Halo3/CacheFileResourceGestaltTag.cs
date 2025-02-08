using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo3
{
    public partial class CacheFileResourceGestaltTag
    {
        public BlockCollection<ResourceEntryBlock> ResourceEntries { get; set; }
        public int FixupDataSize { get; set; }
        public Pointer FixupDataPointer { get; set; }
    }

    public partial class ResourceEntryBlock
    {
        public TagReference OwnerReference { get; set; }
        public ResourceIdentifier ResourcePointer { get; set; }
        public int FixupOffset { get; set; }
        public int FixupSize { get; set; }
        public short LocationType { get; set; }
        public short SegmentIndex { get; set; }
        public BlockCollection<ResourceFixupBlock> ResourceFixups { get; set; }
        public BlockCollection<ResourceDefinitionFixupBlock> ResourceDefinitionFixups { get; set; }

        //prerelease-only properties

        public int CacheIndex { get; set; }
        public int PrimaryOffset { get; set; }
        public int PrimarySize { get; set; }
        public int CacheIndex2 { get; set; }
        public int SecondaryOffset { get; set; }
        public int SecondarySize { get; set; }
    }

    public partial class ResourceFixupBlock
    {
        public int Unknown { get; set; }
        public int Offset { get; set; } //mask 0x0FFFFFFF (4 bit ?, 28 bit offset)

        public int MaskedOffset => Offset & 0x0FFFFFFF;
    }

    public partial class ResourceDefinitionFixupBlock
    {
        public int Offset { get; set; }
        public int Unknown { get; set; }
    }
}
