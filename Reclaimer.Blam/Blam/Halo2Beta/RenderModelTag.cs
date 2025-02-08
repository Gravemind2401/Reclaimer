using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo2;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo2Beta
{
    //note H2B ascii string fields are actually 32 bytes, but the last 4 are not part of the string
    public class RenderModelTag : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public RenderModelTag(IIndexItem item)
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

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

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
                    BaseAddress = s.HeaderSize + 8
                }).ToList()
            };

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);

            model.Meshes.AddRange(Halo2Common.GetMeshes(geoParams, out _));

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
                    MeshRange = (p.LodArray[lod], 1)
                }));

                return region;
            }));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            return model;
        }

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

        internal short[] LodArray => [SuperHighSectionIndex, HighSectionIndex, MediumSectionIndex, LowSectionIndex, SuperLowSectionIndex, PotatoSectionIndex];
    }

    [FixedSize(100, MaxVersion = (int)CacheType.Halo2Beta)]
    [FixedSize(104, MinVersion = (int)CacheType.Halo2Beta)]
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

        [Offset(60, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(64, MinVersion = (int)CacheType.Halo2Beta)]
        public Halo2.DataPointer DataPointer { get; set; }

        [Offset(64, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(68, MinVersion = (int)CacheType.Halo2Beta)]
        public int DataSize { get; set; }

        [Offset(68, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(72, MinVersion = (int)CacheType.Halo2Beta)]
        public int HeaderSize { get; set; }

        [Offset(72, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(76, MinVersion = (int)CacheType.Halo2Beta)]
        public int BodySize { get; set; }

        [Offset(76, MaxVersion = (int)CacheType.Halo2Beta)]
        [Offset(80, MinVersion = (int)CacheType.Halo2Beta)]
        public BlockCollection<Halo2.ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(124)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class NodeBlock
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
        public float InverseScale { get; set; }

        [Offset(72)]
        public Matrix4x4 InverseTransform { get; set; }

        [Offset(120)]
        public float DistanceFromParent { get; set; }
    }

    [FixedSize(44)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class MarkerGroupBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 28)]
        public string Name { get; set; }

        [Offset(32)]
        public BlockCollection<Halo2.MarkerBlock> Markers { get; set; }
    }
}
