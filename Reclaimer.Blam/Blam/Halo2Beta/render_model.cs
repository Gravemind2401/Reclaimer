using Adjutant.Geometry;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo2;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo2Beta
{
    //note H2B ascii string fields are actually 32 bytes, but the last 4 are not part of the string
    public class render_model : ContentTagDefinition, IRenderGeometry
    {
        public render_model(IIndexItem item)
            : base(item)
        { }

        [Offset(52)]
        public BlockCollection<Halo2.BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(64)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(76)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(124)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(148)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(160)]
        public BlockCollection<Halo2.ShaderBlock> Shaders { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 6;

        public IGeometryModel ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);

            var geoParams = new Halo2GeometryArgs
            {
                Cache = Cache,
                Shaders = Shaders,
                IsRenderModel = true,
                Sections = Sections.Select(s => new SectionArgs
                {
                    GeometryClassification = s.GeometryClassification,
                    DataPointer = s.DataPointer,
                    DataSize = s.DataSize,
                    VertexCount = s.VertexCount,
                    FaceCount = s.FaceCount,
                    NodesPerVertex = s.NodesPerVertex,
                    Resources = s.Resources,
                    BoundsIndex = 0,
                    BaseAddress = s.HeaderSize + 8
                }).ToList()
            };

            var model = new GeometryModel(Item.FileName) { CoordinateSystem = CoordinateSystem.Default };

            model.Nodes.AddRange(Nodes);
            model.MarkerGroups.AddRange(MarkerGroups);
            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo2Common.GetMaterials(Shaders));
            model.Meshes.AddRange(Halo2Common.GetMeshes(geoParams));

            foreach (var region in Regions)
            {
                var gRegion = new GeometryRegion { SourceIndex = Regions.IndexOf(region), Name = region.Name };
                gRegion.Permutations.AddRange(region.Permutations.Select(p =>
                    new GeometryPermutation
                    {
                        SourceIndex = region.Permutations.IndexOf(p),
                        Name = p.Name,
                        MeshIndex = p.LodArray[lod],
                        MeshCount = 1
                    }));

                model.Regions.Add(gRegion);
            }

            return model;
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo2.Halo2Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo2.Halo2Common.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [FixedSize(48)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(36)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(44)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
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
    }

    [FixedSize(104)]
    public class SectionBlock
    {
        [Offset(0)]
        public Halo2.GeometryClassification GeometryClassification { get; set; }

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
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
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
        public RealVector3 Position { get; set; }

        [Offset(52)]
        public RealVector4 Rotation { get; set; }

        [Offset(68)]
        public float TransformScale { get; set; }

        [Offset(72)]
        public Matrix4x4 Transform { get; set; }

        [Offset(120)]
        public float DistanceFromParent { get; set; }

        #region IGeometryNode

        string IGeometryNode.Name => Name;

        IVector3 IGeometryNode.Position => Position;

        IVector4 IGeometryNode.Rotation => Rotation;

        Matrix4x4 IGeometryNode.OffsetTransform => Transform;

        #endregion
    }

    [FixedSize(44)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock : IGeometryMarkerGroup
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public BlockCollection<Halo2.MarkerBlock> Markers { get; set; }

        #region IGeometryMarkerGroup

        string IGeometryMarkerGroup.Name => Name;

        IReadOnlyList<IGeometryMarker> IGeometryMarkerGroup.Markers => Markers;

        #endregion
    }
}
