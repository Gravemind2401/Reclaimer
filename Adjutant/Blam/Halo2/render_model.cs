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
    public class render_model
    {
        [Offset(20)]
        public BlockCollection<BoundingBox> BoundingBoxes { get; set; }

        [Offset(28)]
        public BlockCollection<Region> Regions { get; set; }

        [Offset(36)]
        public BlockCollection<Section> Sections { get; set; }

        [Offset(72)]
        public BlockCollection<Node> Nodes { get; set; }

        [Offset(88)]
        public BlockCollection<MarkerGroup> MarkerGroups { get; set; }

        [Offset(96)]
        public BlockCollection<Shader> Shaders { get; set; }
    }

    [FixedSize(56)]
    public class BoundingBox : IRealBounds5D
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(24)]
        public RealBounds UBounds { get; set; }

        [Offset(32)]
        public RealBounds VBounds { get; set; }

        #region IRealBounds5D

        IRealBounds IRealBounds5D.XBounds => XBounds;

        IRealBounds IRealBounds5D.YBounds => YBounds;

        IRealBounds IRealBounds5D.ZBounds => ZBounds;

        IRealBounds IRealBounds5D.UBounds => UBounds;

        IRealBounds IRealBounds5D.VBounds => VBounds; 

        #endregion
    }

    [FixedSize(16)]
    public class Region
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(8)]
        public BlockCollection<Permutation> Permutation { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(16)]
    public class Permutation
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(14)]
        public short PieceIndex { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(92)]
    public class Section
    {
        [Offset(0)]
        public short Type { get; set; }

        [Offset(4)]
        [StoreType(typeof(ushort))]
        public int VertexCount { get; set; }

        [Offset(6)]
        [StoreType(typeof(ushort))]
        public int FaceCount { get; set; }

        [Offset(20)]
        public byte Bones { get; set; }

        [Offset(56)]
        public int RawOffset { get; set; }

        [Offset(60)]
        public int RawSize { get; set; }

        [Offset(68)]
        public int DataSize { get; set; }

        [Offset(72)]
        public BlockCollection<SectionResource> Resources { get; set; }
    }

    [FixedSize(16)]
    public class SectionResource
    {
        public int Type { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }
    }

    [FixedSize(96)]
    public class Node
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short ParentIndex { get; set; }

        [Offset(6)]
        public short FirstChildIndex { get; set; }

        [Offset(8)]
        public short NextSiblingIndex { get; set; }

        [Offset(12)]
        public RealVector3D Position { get; set; }

        [Offset(24)]
        public RealVector4D Rotation { get; set; }

        [Offset(40)]
        public float TransformScale { get; set; }

        [Offset(44)]
        public Matrix4x4 TransformMatrix { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(12)]
    public class MarkerGroup
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<Marker> Markers { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(36)]
    public class Marker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        [Offset(4)]
        public RealVector3D Position { get; set; }

        [Offset(16)]
        public RealVector4D Rotation { get; set; }

        [Offset(32)]
        public float Scale { get; set; }
    }

    [FixedSize(32)]
    public class Shader
    {
        [Offset(12)]
        public TagReference ShaderReference { get; set; }
    }
}
