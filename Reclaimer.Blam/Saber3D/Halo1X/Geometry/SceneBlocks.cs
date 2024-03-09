using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    //0xC003 (unmapped)

    //0x5501 (MaterialListBlock)

    [DataBlock(0x1F02, ExpectedChildCount = 3)] //2002, 2102, 2202
    public class UnknownListBlock0x1F01 : CollectionDataBlock
    {
        public UnknownBoundsBlock0x2002 Bounds => GetUniqueChild<UnknownBoundsBlock0x2002>();
    }

    //apears to be the bounds of geometry that covers the playable area
    [DataBlock(0x2002, ExpectedSize = 48)]
    public class UnknownBoundsBlock0x2002 : DataBlock
    {
        [Offset(0)]
        public int Unknown0 { get; set; }

        [Offset(4)]
        public int Unknown1 { get; set; }

        [Offset(8)]
        public int Unknown2 { get; set; }

        [Offset(12)]
        public RealVector3 MinBounds { get; set; }
        
        [Offset(24)]
        public RealVector3 MaxBounds { get; set; }
        
        [Offset(36)]
        public RealVector4 UnknownVector { get; set; }

        protected override object GetDebugProperties()
        {
            return new
            {
                Unknown0,
                Unknown1,
                Unknown2,
                Min = GetBoundsString(MinBounds),
                Max = GetBoundsString(MaxBounds),
                UnknownVector
            };
        }

        private static string GetBoundsString(RealVector3 value)
        {
            return $"{value.X,7:F2}, {value.Y,7:F2}, {value.Z,7:F2}";
        }
    }

    [DataBlock(0x8204)]
    public class ScriptListBlock : DataBlock
    {
        public int Count { get; set; }
        //public List<StringBlock0xBA01> Scripts { get; } = new List<StringBlock0xBA01>();

        internal override void Read(EndianReader reader)
        {
            Count = reader.ReadInt32();
            //too much data
            //ReadChildren(reader, Count);
            //PopulateChildrenOfType(Scripts);
        }
    }

    //0x8404 (unmapped)

    //0xF000 (node graph)

    //additional geometry, mainly sky objects
    //0xEA01 (unmapped)

    //additional objects, no geometry, lighting related
    //0xB801 (unmapped)

    //0x8002 (unmapped)

    [DataBlock(0x2504, ExpectedSize = 28)]
    public class UnknownBlock0x2504 : DataBlock
    {
        [Offset(0)]
        public int Unknown0 { get; set; }

        [Offset(4)]
        public int Unknown1 { get; set; }

        [Offset(8)]
        public int Unknown2 { get; set; }

        [Offset(12)]
        public int Unknown3 { get; set; }

        [Offset(16)]
        public int Unknown4 { get; set; }

        [Offset(20)]
        public int Unknown5 { get; set; }

        [Offset(24)]
        public int Unknown6 { get; set; }

        protected override object GetDebugProperties() => new { Unknown0, Unknown1, Unknown2, Unknown3, Unknown4, Unknown5, Unknown6 };
    }

    //0x1D02 (appears in Templates too)

    //0x1E02 (unmapped)
}
