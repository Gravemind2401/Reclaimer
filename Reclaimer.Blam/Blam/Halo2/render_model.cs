using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public class render_model : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public render_model(IIndexItem item)
            : base(item)
        { }

        [Offset(20)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(28)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(36)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(72)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(88)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(96)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent());

        private Model GetModelContent()
        {
            const int lod = 0;

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
                    BaseAddress = s.DataSize - s.HeaderSize - 4
                }).ToList()
            };

            var model = new Model { Name = Item.FileName };
            model.Meshes.AddRange(Halo2Common.GetMeshes(geoParams, out _));

            model.Bones.AddRange(Nodes.Select(n => new Bone
            {
                Name = n.Name,
                Transform = n.Transform,
                ParentIndex = n.ParentIndex
            }));

            model.Markers.AddRange(MarkerGroups.Select(g =>
            {
                var marker = new Marker { Name = g.Name };
                marker.Instances.AddRange(g.Markers.Select(m => new MarkerInstance
                {
                    Position = (Vector3)m.Position,
                    Rotation = new Quaternion(m.Rotation.X, m.Rotation.Y, m.Rotation.Z, m.Rotation.W),
                    RegionIndex = m.RegionIndex,
                    PermutationIndex = m.PermutationIndex,
                    BoneIndex = m.NodeIndex
                }));

                return marker;
            }));

            model.Regions.AddRange(Regions.Select(r =>
            {
                var region = new ModelRegion { Name = r.Name };
                region.Permutations.AddRange(r.Permutations.Select(p => new ModelPermutation
                {
                    Name = p.Name,
                    MeshRange = (p.LodArray[lod], 1)
                }));

                return region;
            }));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);

            foreach (var mesh in model.Meshes)
                (mesh.PositionBounds, mesh.TextureBounds) = (posBounds, texBounds);

            return model;
        }

        #endregion
    }

    [FixedSize(56)]
    public class BoundingBoxBlock
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
    }

    [FixedSize(16)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(8)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(16)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short PotatoSectionIndex { get; set; }

        [Offset(6)]
        public short SuperLowSectionIndex { get; set; }

        [Offset(8)]
        public short LowSectionIndex { get; set; }

        [Offset(10)]
        public short MediumSectionIndex { get; set; }

        [Offset(12)]
        public short HighSectionIndex { get; set; }

        [Offset(14)]
        public short SuperHighSectionIndex { get; set; }

        internal short[] LodArray => new[] { SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex, PotatoSectionIndex };
    }

    public enum GeometryClassification : short
    {
        Worldspace = 0,
        Rigid = 1,
        RigidBoned = 2,
        Skinned = 3
    }

    [FixedSize(92)]
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

        [Offset(56)]
        public DataPointer DataPointer { get; set; }

        [Offset(60)]
        public int DataSize { get; set; }

        [Offset(68)]
        public int HeaderSize { get; set; }

        [Offset(72)]
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(96)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class NodeBlock
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
        public RealVector3 Position { get; set; }

        [Offset(24)]
        public RealVector4 Rotation { get; set; }

        [Offset(40)]
        public float TransformScale { get; set; }

        [Offset(44)]
        public Matrix4x4 Transform { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }
    }

    [FixedSize(12)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }
    }

    [FixedSize(36)]
    public class MarkerBlock
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(1)]
        public byte PermutationIndex { get; set; }

        [Offset(2)]
        public byte NodeIndex { get; set; }

        [Offset(4)]
        public RealVector3 Position { get; set; }

        [Offset(16)]
        public RealVector4 Rotation { get; set; }

        [Offset(32)]
        public float Scale { get; set; }

        public override string ToString() => Position.ToString();
    }

    [FixedSize(52, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(32, MinVersion = (int)CacheType.Halo2Xbox)]
    [DebuggerDisplay($"{{{nameof(ShaderReference)},nq}}")]
    public class ShaderBlock
    {
        [Offset(16, MaxVersion = (int)CacheType.Halo2Xbox)]
        [Offset(8, MinVersion = (int)CacheType.Halo2Xbox)]
        public TagReference ShaderReference { get; set; }
    }
}
