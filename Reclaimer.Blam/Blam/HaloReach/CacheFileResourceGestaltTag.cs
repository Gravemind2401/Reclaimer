﻿using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class CacheFileResourceGestaltTag
    {
        [Offset(88, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(100, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<ResourceEntryBlock> ResourceEntries { get; set; }

        [Offset(316, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(328, MinVersion = (int)CacheType.MccHaloReach)]
        public int FixupDataSize { get; set; }

        [Offset(328, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(340, MinVersion = (int)CacheType.MccHaloReach)]
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
