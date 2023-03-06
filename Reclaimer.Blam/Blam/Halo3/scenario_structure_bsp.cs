using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo3
{
    public partial class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public scenario_structure_bsp(IIndexItem item)
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

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.Gen3UnitScale);

        private Model GetModelContent()
        {
            var scenario = Cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var model = new Model { Name = Item.FileName };

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == Item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = Cache.CacheType < CacheType.MccHalo3U4
                ? lightmap.LightmapData.First(lbsp => lbsp.BspIndex == bspIndex)
                : lightmap.LightmapRefs.Where(t => t.TagId >= 0)
                    .Select(lbsp => lbsp.Tag.ReadMetadata<scenario_lightmap_bsp_data>())
                    .FirstOrDefault(lbsp => lbsp.BspIndex == bspIndex)
                    ?? Cache.TagIndex.FirstOrDefault(t => t.ClassCode == "Lbsp" && t.TagName == Item.TagName)?.ReadMetadata<scenario_lightmap_bsp_data>();

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

            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => i.Name))
            {
                var sectionRegion = new ModelRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i => new ModelPermutation
                    {
                        Name = i.Name,
                        Transform = i.Transform,
                        UniformScale = i.TransformScale,
                        MeshRange = (i.SectionIndex, 1),
                        IsInstanced = true
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

        public IEnumerable<IBitmap> GetAllBitmaps() => Halo3Common.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => Halo3Common.GetBitmaps(Shaders, shaderIndexes);

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
