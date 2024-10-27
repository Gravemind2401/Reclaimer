using Reclaimer.IO;

namespace Reclaimer.Blam.Common.Gen5
{
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
