using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo4
{
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class render_model : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public render_model(IIndexItem item)
            : base(item)
        { }

        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public ModelFlags Flags { get; set; }

        [Offset(12)]
        public BlockCollection<RegionBlock> Regions { get; set; }

        [Offset(28)]
        public int InstancedGeometrySectionIndex { get; set; }

        [Offset(32)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(48)]
        public BlockCollection<NodeBlock> Nodes { get; set; }

        [Offset(60)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        [Offset(72)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(312, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(104, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(336, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(128, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(396, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(188, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        [Offset(444, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(248, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            if (Sections.All(s => s.IndexBufferIndex < 0))
                throw Exceptions.GeometryHasNoEdges();

            var geoParams = new Halo4GeometryArgs
            {
                Cache = Cache,
                Shaders = Shaders,
                Sections = Sections,
                NodeMaps = NodeMaps,
                ResourcePointer = ResourcePointer
            };

            var model = new Model { Name = Name, OriginalPath = Item.TagName };
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

            model.Meshes.AddRange(Halo4Common.GetMeshes(geoParams, out var materials));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            CreateInstanceMeshes(model, materials);

            return model;
        }

        private void CreateInstanceMeshes(Model model, List<Material> materials)
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
            region.Permutations.AddRange(GeometryInstances.Select((instance, index) =>
            {
                var permutation = new ModelPermutation
                {
                    Name = instance.Name,
                    Transform = instance.Transform,
                    UniformScale = instance.TransformScale,
                    MeshRange = (InstancedGeometrySectionIndex + GeometryInstances.IndexOf(instance), 1),
                    IsInstanced = true
                };

                var owners = Regions.SelectMany(r => r.Permutations, (r, p) => new { Region = r, Permutation = p })
                    .Where(x => x.Permutation.HasInstance(index))
                    .Select(x => $"{x.Region.Name}\\{x.Permutation.Name}")
                    .ToList();

                permutation.CustomProperties.Add(BlamConstants.GeometryInstancePropertyName, true);
                permutation.CustomProperties.Add(BlamConstants.InstanceNamePropertyName, instance.Name);
                permutation.CustomProperties.Add(BlamConstants.InstanceGroupPropertyName, index);
                permutation.CustomProperties.Add(BlamConstants.PermutationNamePropertyName, owners);

                return permutation;
            }));

            model.Regions.Add(region);

            var sourceMesh = model.Meshes[InstancedGeometrySectionIndex];
            model.Meshes.Remove(sourceMesh);

            var bounds = BoundingBoxes[0];

            var section = Sections[InstancedGeometrySectionIndex];
            for (var i = 0; i < GeometryInstances.Count; i++)
            {
                var subset = section.Subsets[i];
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

                var submesh = section.Submeshes[subset.SubmeshIndex];
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

    [Flags]
    public enum ModelFlags : short
    {
        UseLocalNodes = 4
    }

    [FixedSize(16)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RegionBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(28)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public short SectionIndex { get; set; }

        [Offset(6)]
        public short SectionCount { get; set; }

        [Offset(8)]
        public StringId CloneName { get; set; }

        [Offset(12)]
        public int InstanceMask1 { get; set; }

        [Offset(16)]
        public int InstanceMask2 { get; set; }

        [Offset(20)]
        public int InstanceMask3 { get; set; }

        [Offset(24)]
        public int InstanceMask4 { get; set; }

        public bool HasInstance(int index)
        {
            var maskValue = 1 << (index % 32);
            var flagValue = (index / 32) switch
            {
                0 => InstanceMask1,
                1 => InstanceMask2,
                2 => InstanceMask3,
                3 => InstanceMask4,
                _ => 0
            };

            return (flagValue & maskValue) > 0;
        }
    }

    [FixedSize(60)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public int NodeIndex { get; set; }

        [Offset(8)]
        public float TransformScale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }
    }

    [FixedSize(112)]
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
        public float InverseScale { get; set; }

        [Offset(44)]
        public Matrix4x4 InverseTransform { get; set; }

        [Offset(92)]
        public float DistanceFromParent { get; set; }
    }

    [FixedSize(16)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        [Offset(4)]
        public BlockCollection<MarkerBlock> Markers { get; set; }
    }

    [FixedSize(48)]
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

    [FixedSize(44)]
    [DebuggerDisplay($"{{{nameof(MaterialReference)},nq}}")]
    public class ShaderBlock
    {
        [Offset(0)]
        public TagReference MaterialReference { get; set; }
    }

    [FixedSize(112)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SubmeshBlock> Submeshes { get; set; }

        [Offset(12)]
        public BlockCollection<SubsetBlock> Subsets { get; set; }

        [Offset(24)]
        public short VertexBufferIndex { get; set; }

        [Offset(30)]
        public short UnknownIndex { get; set; }

        [Offset(42)]
        public short IndexBufferIndex { get; set; }

        [Offset(46)]
        public MeshFlags Flags { get; set; }

        [Offset(48)]
        public byte NodeIndex { get; set; }

        [Offset(49)]
        public byte VertexFormat { get; set; }
    }

    [Flags]
    public enum MeshFlags : ushort
    {
        None = 0,
        MeshHasVertexColor = 1 << 0,
        UseRegionIndexForSorting = 1 << 1,
        UseVertexBuffersForIndices = 1 << 2,
        MeshHasPerInstanceLighting = 1 << 3,
        MeshIsUnindexed = 1 << 4,
        SubpartWereMerged = 1 << 5,
        MeshHasFur = 1 << 6,
        MeshHasDecal = 1 << 7,
        MeshDoesntUseCompressedPosition = 1 << 8,
        UseUncompressedVertexFormat = 1 << 9,
        MeshIsPca = 1 << 10,
        MeshCompressionDetermined = 1 << 11,
        MeshHasAuthoredLightmapTextureCoords = 1 << 12,
        MeshHasUsefulSetOfSecondTextureCoords = 1 << 13,
        MeshHasNoLightmap = 1 << 14,
        PerVertexLighting = 1 << 15
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

        [Offset(18)]
        public SubmeshFlags Flags { get; set; }

        [Offset(20)]
        public ushort VertexCount { get; set; }
    }

    [Flags]
    public enum SubmeshFlags : short
    {
        None = 0,
        IsWaterSurface = 1 << 0,
        PerVertexLightmapPart = 1 << 1,
        DebugFlagInstancePart = 1 << 2,
        SubpartsHasUberlightsInfo = 1 << 3,
        DrawCullDistanceMedium = 1 << 4,
        DrawCullDistanceClose = 1 << 5,
        DrawCullRenderingShields = 1 << 6,
        CannotSinglePassRender = 1 << 7,
        IsTransparent = 1 << 8,
        CannotTwoPass = 1 << 9,
        TransparentShouldOutputDepthForDof = 1 << 10,
        DoNotIncludeInStaticLightmap = 1 << 11,
        DoNotIncludeInPvsGeneration = 1 << 12,
        DrawCullRenderingActiveCamo = 1 << 13
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

    [FixedSize(12)]
    public class NodeMapBlock
    {
        [Offset(0)]
        public BlockCollection<byte> Indices { get; set; }
    }
}
