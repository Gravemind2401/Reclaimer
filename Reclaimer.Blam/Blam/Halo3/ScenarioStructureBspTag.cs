using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo3
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
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }

        public ResourceIdentifier ResourcePointer1 { get; set; }

        public BlockCollection<SectionBlock> Sections { get; set; }
        public BlockCollection<BspBoundingBoxBlock> BoundingBoxes { get; set; }

        public ResourceIdentifier ResourcePointer2 { get; set; }
        public ResourceIdentifier ResourcePointer3 { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            var scenario = Cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<ScenarioTag>();

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == Item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<ScenarioLightmapTag>();
            var lightmapData = Cache.CacheType < CacheType.MccHalo3U4
                ? lightmap.LightmapData.First(lbsp => lbsp.BspIndex == bspIndex)
                : lightmap.LightmapRefs.Where(t => t.TagId >= 0)
                    .Select(lbsp => lbsp.Tag.ReadMetadata<ScenarioLightmapBspDataTag>())
                    .FirstOrDefault(lbsp => lbsp.BspIndex == bspIndex)
                    ?? Cache.TagIndex.FirstOrDefault(t => t.ClassCode == "Lbsp" && t.TagName == Item.TagName)?.ReadMetadata<ScenarioLightmapBspDataTag>();

            var geoParams = new Halo3GeometryArgs
            {
                Cache = Cache,
                Shaders = Shaders,
                Sections = lightmapData.Sections,
                ResourcePointer = lightmapData.ResourcePointer
            };

            var clusterRegion = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };
            clusterRegion.Permutations.AddRange(
                Clusters.Select((c, i) => new ModelPermutation
                {
                    Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                    MeshRange = (c.SectionIndex, 1)
                })
            );

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);
            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => i.Name))
            {
                var sectionRegion = new ModelRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i =>
                    {
                        var permutation = new ModelPermutation
                        {
                            Name = i.Name,
                            Transform = i.Transform,
                            UniformScale = i.TransformScale,
                            MeshRange = (i.SectionIndex, 1),
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

            model.Meshes.AddRange(Halo3Common.GetMeshes(geoParams, out _));
            foreach (var i in Enumerable.Range(0, BoundingBoxes.Count))
            {
                if (model.Meshes[i] == null)
                    continue;

                var bounds = BoundingBoxes[i];
                var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);

                (model.Meshes[i].PositionBounds, model.Meshes[i].TextureBounds) = (posBounds, texBounds);
            }

            return model;
        }

        #endregion
    }

    public partial class ClusterBlock
    {
        public RealBounds XBounds { get; set; }
        public RealBounds YBounds { get; set; }
        public RealBounds ZBounds { get; set; }
        public short SectionIndex { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public partial class BspGeometryInstanceBlock
    {
        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public short SectionIndex { get; set; }
        public StringId Name { get; set; }
    }

    public partial class BspBoundingBoxBlock
    {
        public RealBounds XBounds { get; set; }
        public RealBounds YBounds { get; set; }
        public RealBounds ZBounds { get; set; }
        public RealBounds UBounds { get; set; }
        public RealBounds VBounds { get; set; }
    }
}
