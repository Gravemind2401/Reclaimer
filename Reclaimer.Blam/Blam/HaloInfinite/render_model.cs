using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Saber3D.Halo1X.Geometry;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class render_model : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public render_model(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(24)]
        public ResourcePackingPolicy MeshResourcePackingPolicy { get; set; }

        [Offset(40)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(64)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(104)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(124)]
        public BlockCollection<MaterialBlock> Materials { get; set; }

        [Offset(192)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(232)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(252)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }



        public override string ToString() => Item.FileName;

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            var geoParams = new HaloInfiniteGeometryArgs
            {
                Module = Module,
                Regions = Regions,
                ResourcePolicy = MeshResourcePackingPolicy,
                Materials = Materials,
                Sections = Sections,
                NodeMaps = NodeMaps,
                ResourceIndex = Item.ResourceIndex,
                ResourceCount = Item.ResourceCount
            };

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);

            
            model.Bones.AddRange(Nodes.Select(n => new Bone
            {
                Name = n.Name,
                LocalTransform = Utils.CreateMatrix(n.Position, n.Rotation),
                WorldTransform = Utils.CreateWorldMatrix(n.InverseTransform, n.InverseScale),
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
                    MeshRange = (p.SectionIndex, p.SectionCount)
                }));

                return region;
            }));

            //model.Meshes.AddRange(HaloInfiniteCommon.GetMeshes(geoParams, out var materials));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);
            return model;
        }

        #endregion
    }

    public enum ResourcePackingPolicy : int
    {
        SingleResource = 0,
        ResourcePerPermutation = 1
    }

    [FixedSize(24)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(12)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public short SectionIndex { get; set; }

        [Offset(6)]
        public short SectionCount { get; set; }
    }

    [FixedSize(124)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class NodeBlock
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
        public RealVector3 Position { get; set; }

        [Offset(24)]
        public RealVector4 Rotation { get; set; }

        [Offset(40)]
        public Matrix4x4 InverseTransform { get; set; }

        [Offset(88)]
        public float InverseScale { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }
    }

    [FixedSize(24)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }
    }

    [FixedSize(56)]
    public class MarkerBlock
    {
        [Offset(0)]
        public byte RegionIndex { get; set; }

        [Offset(4)]
        public int PermutationIndex { get; set; }

        [Offset(8)]
        public byte NodeIndex { get; set; }

        [Offset(12)]
        public RealVector3 Position { get; set; }

        [Offset(24)]
        public RealVector4 Rotation { get; set; }

        [Offset(40)]
        public float Scale { get; set; }

        [Offset(44)]
        public RealVector3 Direction { get; set; }

        public override string ToString() => Position.ToString();
    }

    [FixedSize(28)]
    [DebuggerDisplay($"{{{nameof(MaterialReference)},nq}}")]
    public class MaterialBlock
    {
        [Offset(0)]
        public TagReference MaterialReference { get; set; }
    }

    [FixedSize(60)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SectionLodBlock> SectionLods { get; set; }

        [Offset(22)]
        public byte NodeIndex { get; set; }

        [Offset(23)]
        public byte VertexFormat { get; set; }

        [Offset(25)]
        [StoreType(typeof(byte))]
        public IndexFormat IndexFormat { get; set; }
    }

    [FixedSize(148)]
    public class SectionLodBlock
    {
        [Offset(40)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }

        [Offset(60)]
        public BlockCollection<SubsetBlock> Subsets { get; set; }

        [Offset(100)]
        public short VertexBufferIndex { get; set; }

        [Offset(138)]
        public short IndexBufferIndex { get; set; }

        [Offset(140)]
        public LodFlags LodFlags { get; set; }
    }

    [FixedSize(24)]
    public class SubmeshBlock
    {
        [Offset(0)]
        public short ShaderIndex { get; set; }

        [Offset(4)]
        public int IndexStart { get; set; }

        [Offset(8)]
        public int IndexLength { get; set; }

        [Offset(12)]
        public ushort SubsetIndex { get; set; }

        [Offset(14)]
        public ushort SubsetCount { get; set; }

        [Offset(20)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(12)]
    public class SubsetBlock
    {
        [Offset(0)]
        public int IndexStart { get; set; }

        [Offset(4)]
        public int IndexLength { get; set; }

        [Offset(8)]
        public ushort SubmeshIndex { get; set; }

        [Offset(10)]
        public ushort VertexCount { get; set; }
    }

    [FixedSize(84)]
    public class BoundingBoxBlock
    {

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
    }

    [FixedSize(20)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }

    public enum LodFlags : ushort
    {
        None = 0,
        Lod0 = 1 << 0,
        Lod1 = 1 << 1,
        Lod2 = 1 << 2,
        Lod3 = 1 << 3,
        Lod4 = 1 << 4,
        Lod5 = 1 << 5,
        Lod6 = 1 << 6,
        Lod7 = 1 << 7,
        Lod8 = 1 << 8,
        Lod9 = 1 << 9,
        Lod10 = 1 << 10,
        Lod11 = 1 << 11,
        Lod12 = 1 << 12,
        Lod13 = 1 << 13,
        Lod14 = 1 << 14,
        Lod15 = 1 << 15,
    }
}
