using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2Beta
{
    public class render_model
    //note H2B ascii string fields are actually 32 bytes, but the last 4 are not part of the string
    {
        private readonly IIndexItem item;

        public render_model(IIndexItem item)
        {
            this.item = item;
        }

        [Offset(52)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(64)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(76)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(124)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(148)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(160)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }
    public struct MeshResourceDetailsBlock
    {
        [Offset(52)]
        public ushort IndexCount { get; set; }

        [Offset(168)]
        public ushort NodeMapCount { get; set; }
    }

    [FixedSize(72)]
    public struct SubmeshDataBlock
    {
        [Offset(4)]
        public short ShaderIndex { get; set; }

        [Offset(6)]
        public ushort IndexStart { get; set; }

        [Offset(8)]
        public ushort IndexLength { get; set; }
    }

    [FixedSize(56)]
    public class BoundingBoxBlock : IRealBounds5D
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

    [FixedSize(48)]
    public class RegionBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(36)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(44)]
    public class PermutationBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public short PotatoSectionIndex { get; set; }

        [Offset(34)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(36)]
        public short LowSectionIndex { get; set; }

        [Offset(38)]
        public short MediumSectionIndex { get; set; }

        [Offset(40)]
        public short HighSectionIndex { get; set; }

        [Offset(42)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex, PotatoSectionIndex };

        public override string ToString() => Name;
    }

    public enum GeometryClassification : short
    {
        Worldspace = 0,
        Rigid = 1,
        RigidBoned = 2,
        Skinned = 3
    }

    [FixedSize(104)]
    public class SectionBlock
    {
        [Offset(0)]
        public GeometryClassification GeometryClassification { get; set; }

        [Offset(4)]
        public ushort VertexCount { get; set; }

        [Offset(6)]
        public ushort FaceCount { get; set; }

        [Offset(20)]
        public byte NodesPerVertex { get; set; }

        [Offset(64)]
        public Halo2.DataPointer DataPointer { get; set; }

        [Offset(68)]
        public int DataSize { get; set; }

        [Offset(72)]
        public int HeaderSize { get; set; }

        [Offset(76)]
        public int BodySize { get; set; }

        [Offset(80)]
        public BlockCollection<Halo2.ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(124)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public short ParentIndex { get; set; }

        [Offset(34)]
        public short FirstChildIndex { get; set; }

        [Offset(36)]
        public short NextSiblingIndex { get; set; }

        [Offset(38)]
        public short SomethingIndex { get; set; }

        [Offset(40)]
        public RealVector3D Position { get; set; }

        [Offset(52)]
        public RealVector4D Rotation { get; set; }

        [Offset(68)]
        public float TransformScale { get; set; }

        [Offset(72)]
        public Matrix4x4 Transform { get; set; }

        [Offset(120)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation;

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

        #endregion
    }

    [FixedSize(44)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public BlockCollection<MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(36)]
    public class MarkerBlock : IGeometryMarker
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

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(36)]
    public class ShaderBlock
    {
        [Offset(4)]
        public TagReference ShaderReference { get; set; }

        public override string ToString() => ShaderReference.Tag?.FullPath;
    }
}
