using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class RenderModelTag : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public RenderModelTag(ModuleItem item, MetadataHeader header)
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

        [Offset(316)]
        public int TotalIndexBufferCount { get; set; }

        [Offset(318)]
        public int TotalVertexBufferCount { get; set; }

        [Offset(324)]
        public BlockCollection<MeshResourceGroupBlock> MeshResourceGroups { get; set; }

        public override string ToString() => Item.FileName;

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        public Model GetModelContent()
        {
            var geoParams = new HaloInfiniteGeometryArgs
            {
                Module = Module,
                Regions = Regions,
                ResourcePolicy = MeshResourcePackingPolicy,
                Materials = Materials,
                Sections = Sections,
                NodeMaps = NodeMaps,
                MeshResourceGroups = MeshResourceGroups,
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

            model.Meshes.AddRange(HaloInfiniteCommon.GetMeshes(geoParams, out var materials));

            // Bounding might not exist in some cases (attachments?)
            if (BoundingBoxes.Count != 0)
            {
                var bounds = BoundingBoxes[0];
                var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
                model.SetCompressionBounds(posBounds, texBounds);
            }

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
        public StringHashGen5 Name { get; set; }

        [Offset(4)]
        public BlockCollection<PermutationBlock> Permutations { get; set; }
    }

    [FixedSize(12)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class PermutationBlock
    {
        [Offset(0)]
        public StringHashGen5 Name { get; set; }

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
        public StringHashGen5 Name { get; set; }

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
        public StringHashGen5 Name { get; set; }

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
        public TagReferenceGen5 MaterialReference { get; set; }
    }

    [FixedSize(60)]
    public class SectionBlock
    {
        [Offset(0)]
        public BlockCollection<SectionLodBlock> SectionLods { get; set; }

        [Offset(20)]
        public MeshFlags Flags { get; set; }

        [Offset(22)]
        public byte NodeIndex { get; set; }

        [Offset(23)]
        public VertexType VertexFormat { get; set; }

        [Offset(24)]
        [StoreType(typeof(byte))]
        public bool UseDualQuat { get; set; }

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
        public VertexBufferIndexArray VertexBufferIndicies { get; set; }

        [Offset(138)]
        public short IndexBufferIndex { get; set; }

        [Offset(140)]
        public LodFlags LodFlags { get; set; }

        [Offset(142)]
        public ushort LODHasShadowProxies { get; set; }
    }

    [FixedSize(38)]
    public class VertexBufferIndexArray : IList<short>
    {
        //this class is here to avoid having 19 separate "vertex buffer index" properties
        //(and because I never implemented array properties in the dynamic IO)

        private readonly short[] indices;

        public VertexBufferIndexArray(EndianReader reader)
        {
            indices = reader.ReadArray<short>(19);
        }

        public IEnumerable<short> ValidIndicies => indices.Where(i => i >= 0);

        #region IList<short> Implementation
        public short this[int index]
        {
            get => ((IList<short>)indices)[index];
            set => ((IList<short>)indices)[index] = value;
        }

        public int Count => ((ICollection<short>)indices).Count;

        public bool IsReadOnly => ((ICollection<short>)indices).IsReadOnly;

        public void Add(short item) => ((ICollection<short>)indices).Add(item);
        public void Clear() => ((ICollection<short>)indices).Clear();
        public bool Contains(short item) => ((ICollection<short>)indices).Contains(item);
        public void CopyTo(short[] array, int arrayIndex) => ((ICollection<short>)indices).CopyTo(array, arrayIndex);
        public IEnumerator<short> GetEnumerator() => ((IEnumerable<short>)indices).GetEnumerator();
        public int IndexOf(short item) => ((IList<short>)indices).IndexOf(item);
        public void Insert(int index, short item) => ((IList<short>)indices).Insert(index, item);
        public bool Remove(short item) => ((ICollection<short>)indices).Remove(item);
        public void RemoveAt(int index) => ((IList<short>)indices).RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => indices.GetEnumerator();
        #endregion
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

    [FixedSize(16)]
    public class MeshResourceGroupBlock
    {
        [LoadFromStructureDefinition("8aeb8021-4164-f60a-9170-0c9745553623")]
        public RenderGeometryApiResource RenderGeometryApiResource { get; set; }
    }

    [Flags]
    public enum LodFlags : ushort
    {
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
        Lod15 = 1 << 15
    }

    [Flags]
    public enum MeshFlags : ushort
    {
        None = 0,
        MeshHasVertexColor = 1 << 0,
        UseRegionIndexForSorting = 1 << 1,
        CanBeRenderedInDrawBundles = 1 << 2,
        MeshIsCustomShadowCaster = 1 << 3,
        MeshIsUnindexed = 1 << 4,
        MeshShouldRenderInZPrePass = 1 << 5,
        UseUncompressedVertexFormat = 1 << 6,
        MeshIsPCA = 1 << 7,
        MeshHasUsefulUV2 = 1 << 8,
        MeshHasUsefulUV3 = 1 << 9,
        UseUV3TangentRotation = 1 << 10
    }

    [FixedSize(312)]
    public class RenderGeometryApiResource
    {
        [Offset(0)]
        public BlockCollection<RasterizerVertexBuffer> PcVertexBuffers { get; set; }

        [Offset(20)]
        public BlockCollection<RasterizerIndexBuffer> PcIndexBuffers { get; set; }
    }

    [FixedSize(80)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class RasterizerVertexBuffer
    {
        [Offset(0)]
        public VertexBufferUsage Usage { get; set; }

        [Offset(4)]
        public RasterizerVertexFormat Format { get; set; }

        [Offset(8)]
        public byte Stride { get; set; }

        [Offset(12)]
        public int Count { get; set; }

        [Offset(16)]
        public int Offset { get; set; }

        public int DataLength => Stride * Count;

        private string GetDebuggerDisplay() => $"{Usage} [{Format}]";
    }

    [FixedSize(72)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class RasterizerIndexBuffer
    {
        [Offset(0)]
        [StoreType(typeof(byte))]
        public IndexFormat DeclarationType { get; set; } //not sure if this is actually index format but it matches so far

        [Offset(1)]
        public byte Stride { get; set; }

        [Offset(4)]
        public int Count { get; set; }

        [Offset(8)]
        public int Offset { get; set; }

        public int DataLength => Stride * Count;

        private string GetDebuggerDisplay() => $"{DeclarationType}[{Count}]";
    }

    public enum VertexType : byte
    {
        World,
        Rigid,
        Skinned,
        ParticleModel,
        Screen,
        Debug,
        Transparent,
        Particle,
        Removed08,
        Removed09,
        ChudSimple,
        Decorator,
        PositionOnly,
        Removed13,
        Ripple,
        Removed15,
        TessellatedTerrain,
        Empty,
        Decal,
        Removed19,
        Removed20,
        PositionOnly2,
        Tracer,
        RigidBoned,
        Removed24,
        CheapParticle,
        DqSkinned,
        Skinned8Weights,
        TessellatedVector,
        Interaction,
        NumberOfStandardVertexTypes
    }

    public enum VertexBufferUsage : int
    {
        Position,
        UV0,
        UV1,
        UV2,
        Color,
        Normal,
        Tangent,
        BlendIndices0,
        BlendWeights0,
        BlendIndices1,
        BlendWeights1,
        PrevPosition,
        InstanceData,
        BlendshapePosition,
        BlendshapeNormal,
        BlendshapeIndex,
        Edge,
        EdgeIndex,
        EdgeIndexInfo
    }

    public enum RasterizerVertexFormat : int
    {
        Real,
        RealVector2D,
        RealVector3D,
        RealVector4D,
        ByteVector4D,
        ByteARGBColor,
        ShortVector2D,
        ShortVector2DNormalized,
        ShortVector4DNormalized,
        WordVector2DNormalized,
        WordVector4DNormalized,
        Real16Vector2D,
        Real16Vector4D,
        _10_10_10_Normalized,
        _10_10_10_2,
        _10_10_10_2_SignedNormalizedPackedAsUnorm,
        Dword,
        DwordVector2D,
        _11_11_10_Float,
        ByteUnitVector3D,
        WordVector3DNormalizedWith4Word,
    }
}
