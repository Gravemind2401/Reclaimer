using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo2
{
    public partial class ScenarioStructureBspTag : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public ScenarioStructureBspTag(IIndexItem item)
            : base(item)
        { }

        public RealBounds XBounds { get; set; }
        public RealBounds YBounds { get; set; }
        public RealBounds ZBounds { get; set; }

        public BlockCollection<ClusterBlock> Clusters { get; set; }
        public BlockCollection<ShaderBlock> Shaders { get; set; }
        public BlockCollection<BspSectionBlock> Sections { get; set; }
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

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
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

    public partial class ClusterBlock
    {
        public ushort VertexCount { get; set; }
        public ushort FaceCount { get; set; }
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }
        public DataPointer DataPointer { get; set; }
        public int DataSize { get; set; }
        public int HeaderSize { get; set; }
        public BlockCollection<ResourceInfoBlock> Resources { get; set; }
    }

    public partial class BspSectionBlock : ClusterBlock
    {

    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public partial class GeometryInstanceBlock
    {
        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public short SectionIndex { get; set; }
        public StringId Name { get; set; }
    }
}
