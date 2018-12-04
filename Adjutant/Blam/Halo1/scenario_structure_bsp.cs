using Adjutant.IO;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class scenario_structure_bsp
    {
        [Offset(224)]
        public RealBounds XBounds { get; set; }

        [Offset(232)]
        public RealBounds YBounds { get; set; }

        [Offset(240)]
        public RealBounds ZBounds { get; set; }

        [Offset(272)]
        public int SurfaceCount { get; set; }

        [Offset(276)]
        public Pointer SurfacePointer { get; set; }

        [Offset(284)]
        public BlockCollection<Lightmap> Lightmaps { get; set; }

        [Offset(600)]
        public BlockCollection<BSPMarker> Markers { get; set; }
    }

    [FixedSize(32)]
    public class Lightmap
    {
        [Offset(20)]
        public BlockCollection<Material> Materials { get; set; }
    }

    [FixedSize(256)]
    public class Material
    {
        [Offset(12)]
        public TagReference ShaderReference { get; set; }

        [Offset(20)]
        public int SurfaceIndex { get; set; }

        [Offset(24)]
        public int SurfaceCount { get; set; }

        [Offset(180)]
        public int VertexCount { get; set; }

        [Offset(228)]
        public Pointer VertexPointer { get; set; }

        public override string ToString() => ShaderReference.ToString();
    }

    [FixedSize(104)]
    public class Cluster
    {
        [Offset(52)]
        public BlockCollection<Subcluster> Subclusters { get; set; }

        [Offset(68)]
        public BlockCollection<int> SurfaceIndices { get; set; }
    }

    [FixedSize(36)]
    public class Subcluster
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(24)]
        public BlockCollection<int> SurfaceIndices { get; set; }
    }

    [FixedSize(60)]
    public class BSPMarker
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string Name { get; set; }

        [Offset(32)]
        public RealVector4D Rotation { get; set; }

        [Offset(48)]
        public RealVector3D Position { get; set; }
    }
}
