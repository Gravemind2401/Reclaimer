using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        [Offset(68)]
        public RealBounds XBounds { get; set; }

        [Offset(76)]
        public RealBounds YBounds { get; set; }

        [Offset(84)]
        public RealBounds ZBounds { get; set; }

        [Offset(172)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(180)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(328)]
        public BlockCollection<BspSectionBlock> Sections { get; set; }

        [Offset(336)]
        public BlockCollection<GeometryInstanceBlock> GeometryInstances { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            var geoParams = new Halo2GeometryArgs
            {
                Cache = Cache,
                Shaders = Shaders,
                Sections = Clusters.Select(c => new SectionArgs
                {
                    DataPointer = c.DataPointer,
                    DataSize = c.DataSize,
                    VertexCount = c.VertexCount,
                    FaceCount = c.FaceCount,
                    Resources = c.Resources,
                    BaseAddress = c.HeaderSize + 8
                }).Concat(Sections.Select(s => new SectionArgs
                {
                    DataPointer = s.DataPointer,
                    DataSize = s.DataSize,
                    VertexCount = s.VertexCount,
                    FaceCount = s.FaceCount,
                    Resources = s.Resources,
                    BaseAddress = s.HeaderSize + 8
                })).ToList()
            };

            var model = new Model { Name = Item.FileName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);
            model.Meshes.AddRange(Halo2Common.GetMeshes(geoParams, out _));

            var clusterRegion = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };

            foreach (var section in Clusters.Where(s => s.VertexCount > 0))
            {
                var sectionIndex = Clusters.IndexOf(section);

                var perm = new ModelPermutation
                {
                    Name = sectionIndex.ToString("D3", CultureInfo.CurrentCulture),
                    MeshRange = (sectionIndex, 1)
                };

                clusterRegion.Permutations.Add(perm);
            }

            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => i.Name))
            {
                var sectionRegion = new ModelRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Where(i => Sections.ElementAtOrDefault(i.SectionIndex)?.VertexCount > 0)
                    .Select(i =>
                    {
                        var permutation = new ModelPermutation
                        {
                            Name = i.Name,
                            Transform = i.Transform,
                            UniformScale = i.TransformScale,
                            MeshRange = (Clusters.Count + i.SectionIndex, 1),
                            IsInstanced = true
                        };

                        permutation.CustomProperties.Add(BlamConstants.GeometryInstancePropertyName, true);
                        permutation.CustomProperties.Add(BlamConstants.InstanceNamePropertyName, i.Name);
                        permutation.CustomProperties.Add(BlamConstants.InstanceGroupPropertyName, i.SectionIndex);
                        permutation.CustomProperties.Add(BlamConstants.PermutationNamePropertyName, (string)null);

                        return permutation;
                    })
                );
                model.Regions.Add(sectionRegion);
            }

            return model;
        }

        #endregion
    }

    [FixedSize(176)]
    public class ClusterBlock
    {
        [Offset(0)]
        public ushort VertexCount { get; set; }

        [Offset(2)]
        public ushort FaceCount { get; set; }

        [Offset(24)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(40)]
        public DataPointer DataPointer { get; set; }

        [Offset(44)]
        public int DataSize { get; set; }

        [Offset(48)]
        public int HeaderSize { get; set; }

        [Offset(56)]
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    [FixedSize(200)]
    public class BspSectionBlock : ClusterBlock
    {

    }

    [FixedSize(88)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class GeometryInstanceBlock
    {
        [Offset(0)]
        public float TransformScale { get; set; }

        [Offset(4)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }

        [Offset(80)]
        public StringId Name { get; set; }
    }
}
