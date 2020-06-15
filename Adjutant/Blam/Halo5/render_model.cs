using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    public class render_model
    {
        private readonly Module module;
        private readonly ModuleItem item;

        public MetadataHeader Header { get; }

        public render_model(Module module, ModuleItem item, MetadataHeader header)
        {
            this.module = module;
            this.item = item;

            Header = header;
        }

        [Offset(32)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(60)]
        public int InstancedGeometrySectionIndex { get; set; }

        [Offset(64)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(96)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(152)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(180)]
        public BlockCollection<MaterialBlock> Materials { get; set; }

        [Offset(328)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(356)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }
    }

    [FixedSize(32)]
    public class RegionBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(28)]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public short SectionIndex { get; set; }

        [Offset(6)]
        public short SectionCount { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(60)]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public int NodeIndex { get; set; }

        [Offset(8)]
        public float TransformScale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(124)]
    public class NodeBlock : IGeometryNode
    {
        [Offset(0)]
        public StringHash Name { get; set; }

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
        public Matrix4x4 Transform { get; set; }

        [Offset(88)]
        public float TransformScale { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }

        public override string ToString() => Name;

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IRealVector3D IGeometryNode.Position => Position;

        IRealVector4D IGeometryNode.Rotation => Rotation;

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

        #endregion
    }

    [FixedSize(32)]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }

        public override string ToString() => Name;

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }

    [FixedSize(56)]
    public class MarkerBlock : IGeometryMarker
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(4)]
        public int PermutationIndex { get; set; }

        [Offset(8)]
        public byte NodeIndex { get; set; }

        [Offset(12)]
        public RealVector3D Position { get; set; }

        [Offset(24)]
        public RealVector4D Rotation { get; set; }

        [Offset(40)]
        public float Scale { get; set; }

        [Offset(44)]
        public RealVector3D Direction { get; set; }

        public override string ToString() => Position.ToString();

        #region IGeometryMarker

        byte IGeometryMarker.PermutationIndex => (byte)PermutationIndex;

        IRealVector3D IGeometryMarker.Position => Position;

        IRealVector4D IGeometryMarker.Rotation => Rotation;

        #endregion
    }

    [FixedSize(32)]
    public class MaterialBlock
    {
        //[Offset(0)]
        //public TagReference MaterialReference { get; set; }

        //public override string ToString() => MaterialReference.Tag?.FullPath;
    }

    [FixedSize(52)]
    public class BoundingBoxBlock : IRealBounds5D
    {
        //short flags, short padding

        [Offset(4)]
        public RealBounds XBounds { get; set; }

        [Offset(12)]
        public RealBounds YBounds { get; set; }

        [Offset(20)]
        public RealBounds ZBounds { get; set; }

        [Offset(28)]
        public RealBounds UBounds { get; set; }

        [Offset(36)]
        public RealBounds VBounds { get; set; }

        #region IRealBounds5D

        IRealBounds IRealBounds5D.XBounds => XBounds;

        IRealBounds IRealBounds5D.YBounds => YBounds;

        IRealBounds IRealBounds5D.ZBounds => ZBounds;

        IRealBounds IRealBounds5D.UBounds => UBounds;

        IRealBounds IRealBounds5D.VBounds => VBounds;

        #endregion
    }

    [FixedSize(28)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }
}
