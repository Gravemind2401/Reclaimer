using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public class render_model : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public render_model(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(24)]
        public ResourcePackingPolicy MeshResourcePackingPolicy { get; set; }

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

        [Offset(272)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(328)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(356)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        public override string ToString() => Item.FileName;

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            const int lod = 0;

            var geoParams = new Halo5GeometryArgs
            {
                Module = Module,
                ResourcePolicy = MeshResourcePackingPolicy,
                Regions = Regions,
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

            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            CreateInstanceMeshes(model, materials, lod);

            return model;
        }

        private void CreateInstanceMeshes(Model model, List<Material> materials, int lod)
        {
            if (InstancedGeometrySectionIndex < 0)
                return;

            /* 
             * The render_model geometry instances have all their mesh data
             * in the same section and each instance has its own subset.
             * This function separates the subsets into separate sections
             * to make things easier for the model rendering and exporting 
             */

            var region = new ModelRegion { Name = BlamConstants.ModelInstancesGroupName };
            region.Permutations.AddRange(GeometryInstances.Select(i =>
                new ModelPermutation
                {
                    Name = i.Name,
                    Transform = i.Transform,
                    UniformScale = i.TransformScale,
                    MeshRange = (InstancedGeometrySectionIndex + GeometryInstances.IndexOf(i), 1)
                }));

            model.Regions.Add(region);

            var sourceMesh = model.Meshes[InstancedGeometrySectionIndex];
            model.Meshes.Remove(sourceMesh);

            var bounds = BoundingBoxes[0];

            var section = Sections[InstancedGeometrySectionIndex];
            var localLod = Math.Min(lod, section.SectionLods.Count - 1);
            for (var i = 0; i < GeometryInstances.Count; i++)
            {
                var subset = section.SectionLods[localLod].Subsets[i];
                var mesh = new Mesh
                {
                    BoneIndex = (byte)GeometryInstances[i].NodeIndex,
                    PositionBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds),
                    TextureBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds)
                };

                var strip = sourceMesh.IndexBuffer.GetSubset(subset.IndexStart, subset.IndexLength);

                var min = strip.Min();
                var max = strip.Max();
                var len = max - min + 1;

                mesh.IndexBuffer = IndexBuffer.Transform(sourceMesh.IndexBuffer.Slice(subset.IndexStart, subset.IndexLength), -min);
                mesh.VertexBuffer = sourceMesh.VertexBuffer.Slice(min, len);

                var submesh = section.SectionLods[localLod].Submeshes[subset.SubmeshIndex];
                mesh.Segments.Add(new MeshSegment
                {
                    Material = materials.ElementAtOrDefault(submesh.ShaderIndex),
                    IndexStart = 0,
                    IndexLength = mesh.IndexBuffer.Count
                });

                model.Meshes.Add(mesh);
            }
        }

        #endregion
    }

    public enum ResourcePackingPolicy : int
    {
        SingleResource = 0,
        ResourcePerPermutation = 1
    }

    [FixedSize(32)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(28)]
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

    [FixedSize(60)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
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

    [FixedSize(32)]
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

    [FixedSize(32)]
    [DebuggerDisplay($"{{{nameof(MaterialReference)},nq}}")]
    public class MaterialBlock
    {
        [Offset(0)]
        public TagReference MaterialReference { get; set; }
    }

    [FixedSize(128)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SectionLodBlock> SectionLods { get; set; }

        [Offset(30)]
        public byte NodeIndex { get; set; }

        [Offset(31)]
        public byte VertexFormat { get; set; }

        [Offset(32)]
        [StoreType(typeof(byte))]
        public bool UseDualQuat { get; set; }

        [Offset(33)]
        [StoreType(typeof(byte))]
        public IndexFormat IndexFormat { get; set; }
    }

    [FixedSize(140)]
    public class SectionLodBlock
    {
        [Offset(56)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }

        [Offset(84)]
        public BlockCollection<SubsetBlock> Subsets { get; set; }

        [Offset(112)]
        public short VertexBufferIndex { get; set; }

        [Offset(134)]
        public short IndexBufferIndex { get; set; }

        [Offset(136)]
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

    [FixedSize(16)]
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

    [FixedSize(52)]
    public class BoundingBoxBlock
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
    }

    [FixedSize(28)]
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
