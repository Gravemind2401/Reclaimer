using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    [FixedSize(80)]
    public class TagHeader
    {
        [Offset(0)]
        public int Header { get; set; }

        [Offset(4)]
        [VersionNumber]
        public ModuleType Version { get; set; }

        [Offset(16)]
        public long AssetChecksum { get; set; }

        [Offset(28)]
        public int DependencyCount { get; set; }

        [Offset(32)]
        public int DataBlockCount { get; set; }

        [Offset(36)]
        public int TagStructureCount { get; set; }

        [Offset(40)]
        public int DataReferenceCount { get; set; }

        [Offset(44)]
        public int TagReferenceCount { get; set; }

        [Offset(48)]
        public int StringIdCount { get; set; }

        [Offset(52)]
        public int StringTableSize { get; set; }

        [Offset(56)]
        public int ZonesetDataSize { get; set; }

        [Offset(60)]
        public int HeaderSize { get; set; }

        [Offset(64)]
        public int DataSize { get; set; }

        [Offset(68)]
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
