using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class cache_file_resource_gestalt
    {
        [Offset(36, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(88, MinVersion = (int)CacheType.Halo3Retail)]
        public BlockCollection<ResourceEntryBlock> ResourceEntries { get; set; }

        [Offset(132, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(316, MinVersion = (int)CacheType.Halo3Retail)]
        public int FixupDataSize { get; set; }

        [Offset(144, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(328, MinVersion = (int)CacheType.Halo3Retail)]
        public Pointer FixupDataPointer { get; set; }
    }

    [FixedSize(96, MaxVersion = (int)CacheType.Halo3Retail)]
    [FixedSize(64, MinVersion = (int)CacheType.Halo3Retail)]
    public class ResourceEntryBlock
    {
        [Offset(0)]
        public TagReference OwnerReference { get; set; }

        [Offset(16)]
        public ResourceIdentifier ResourcePointer { get; set; }

        [Offset(24, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(20, MinVersion = (int)CacheType.Halo3Retail)]
        public int FixupOffset { get; set; }

        [Offset(28, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(24, MinVersion = (int)CacheType.Halo3Retail)]
        public int FixupSize { get; set; }

        [Offset(32)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public short LocationType { get; set; }

        [Offset(34)]
        [MinVersion((int)CacheType.Halo3Retail)]
        public short SegmentIndex { get; set; }

        [Offset(36)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int CacheIndex { get; set; }

        [Offset(40)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int RequiredOffset { get; set; }

        [Offset(44)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int RequiredSize { get; set; }

        [Offset(52)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int CacheIndex2 { get; set; }

        [Offset(56)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int OptionalOffset { get; set; }

        [Offset(60)]
        [VersionSpecific((int)CacheType.Halo3Beta)]
        public int OptionalSize { get; set; }

        [Offset(72, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(40, MinVersion = (int)CacheType.Halo3Retail)]
        public BlockCollection<ResourceFixupBlock> ResourceFixups { get; set; }

        [Offset(84, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(52, MinVersion = (int)CacheType.Halo3Retail)]
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
