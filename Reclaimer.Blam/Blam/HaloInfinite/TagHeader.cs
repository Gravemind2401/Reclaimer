using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo5;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    [FixedSize(80)]
    public class TagHeader
    {
        [Offset(0)]
        public int Header { get; set; }
        public ModuleType Version { get; set; } = ModuleType.HaloInfinite;

        [Offset(8)]
        public long AssetHash { get; set; }

        [Offset(16)]
        public long AssetChecksum { get; set; }

        [Offset(24)]
        public int DependencyCount { get; set; }

        [Offset(28)]
        public int DataBlockCount { get; set; }

        [Offset(32)]
        public int TagStructureCount { get; set; }

        [Offset(36)]
        public int DataReferenceCount { get; set; }

        [Offset(40)]
        public int TagReferenceCount { get; set; }

        [Offset(44)]
        public int StringTableSize { get; set; }

        [Offset(48)]
        public int ZonesetDataSize { get; set; }

        [Offset(56)]
        public int HeaderSize { get; set; }

        [Offset(60)]
        public int DataSize { get; set; }

        [Offset(64)]
        public int ResourceDataSize { get; set; }
    }

    [FixedSize(24)]
    public class TagDependency
    {
        [Offset(0)]
        public int ClassId { get; set; }

        [Offset(4)]
        public int NameOffset { get; set; }

        [Offset(8)]
        public long AssetId { get; set; }

        [Offset(16)]
        public int GlobalId { get; set; }

        [Offset(20)]
        public int ParentId { get; set; }
    }

    [FixedSize(16)]
    public class DataBlock
    {
        [Offset(0)]
        public int Size { get; set; }

        [Offset(6)]
        public short Section { get; set; }

        [Offset(8)]
        public long Offset { get; set; }
    }

    [FixedSize(32)]
    public class TagStructureDefinition
    {
        [Offset(0)]
        public Guid Guid { get; set; }

        [Offset(16)]
        public StructureType Type { get; set; }

        [Offset(20)]
        public int TargetIndex { get; set; }

        [Offset(24)]
        public int FieldBlock { get; set; }

        [Offset(28)]
        public int FieldOffset { get; set; }
    }

    [FixedSize(20)]
    public class DataBlockReference
    {
        [Offset(0)]
        public int ParentStructureIndex { get; set; }

        [Offset(8)]
        public int TargetIndex { get; set; }

        [Offset(12)]
        public int FieldBlock { get; set; }

        [Offset(16)]
        public int FieldOffset { get; set; }
    }

    [FixedSize(16)]
    public class TagBlockReference
    {
        [Offset(0)]
        public int FieldBlock { get; set; }

        [Offset(4)]
        public int FieldOffset { get; set; }

        [Offset(8)]
        public int NameOffset { get; set; }

        [Offset(12)]
        public int DependencyIndex { get; set; }
    }
}
