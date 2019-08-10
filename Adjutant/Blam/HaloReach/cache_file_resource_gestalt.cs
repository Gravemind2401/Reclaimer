using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.HaloReach
{
    public class cache_file_resource_gestalt
    {
        [Offset(88)]
        public BlockCollection<ResourceEntryBlock> ResourceEntries { get; set; }

        [Offset(316)]
        public int FixupDataSize { get; set; }

        [Offset(328)]
        public Pointer FixupDataPointer { get; set; }
    }

    [FixedSize(64)]
    public class ResourceEntryBlock
    {
        [Offset(0)]
        public TagReference OwnerReference { get; set; }

        [Offset(16)]
        public ResourceIdentifier ResourcePointer { get; set; }

        [Offset(20)]
        public int FixupOffset { get; set; }

        [Offset(24)]
        public int FixupSize { get; set; }

        [Offset(32)]
        public short LocationType { get; set; }

        [Offset(34)]
        public short SegmentIndex { get; set; }

        [Offset(40)]
        public BlockCollection<ResourceFixupBlock> ResourceFixups { get; set; }

        [Offset(52)]
        public BlockCollection<ResourceDefinitionFixupBlock> ResourceDefinitionFixups { get; set; }
    }

    [FixedSize(8)]
    public class ResourceFixupBlock
    {
        [Offset(0)]
        public int Unknown { get; set; }

        [Offset(4)]
        public int Offset { get; set; } //mask 0x0FFFFFFF (4 bit ?, 28 bit offset)
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
