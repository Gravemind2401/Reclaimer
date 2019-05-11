using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class scenario_structure_bsp
    {
        [Offset(68)]
        public RealBounds XBounds { get; set; }

        [Offset(76)]
        public RealBounds YBounds { get; set; }

        [Offset(84)]
        public RealBounds ZBounds { get; set; }

        [Offset(172)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(180)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(328)]
        public BlockCollection<BspSectionBlock> Sections { get; set; }

        [Offset(336)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }
    }

    [FixedSize(176)]
    public class ClusterBlock
    {
        [Offset(0)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }

        [Offset(2)]
        [StoreType(typeof(ushort))]
        public int FaceCount { get; set; }

        [Offset(24)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(40)]
        public int DataOffset { get; set; }

        [Offset(44)]
        public int DataSize { get; set; }

        [Offset(48)]
        public int HeaderSize { get; set; }

        [Offset(56)]
        public BlockCollection<SectionResourceBlock> Resources { get; set; }
    }

    [FixedSize(200)]
    public class BspSectionBlock : ClusterBlock
    {

    }

    [FixedSize(88)]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public float Scale { get; set; }

        [Offset(4)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }

        [Offset(80)]
        public StringId Name { get; set; }

        public override string ToString() => Name;
    }
}
