using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo3
{
    public partial class scenario_structure_bsp : ContentTagDefinition, IRenderGeometry
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

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            Exceptions.ThrowIfIndexOutOfRange(lod, ((IRenderGeometry)this).LodCount);

            var scenario = Cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var model = new GeometryModel(Item.FileName) { CoordinateSystem = CoordinateSystem.Default };

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == Item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = Cache.CacheType < CacheType.MccHalo3U4
                ? lightmap.LightmapData.First(lbsp => lbsp.BspIndex == bspIndex)
                : lightmap.LightmapRefs.Where(t => t.TagId >= 0)
                    .Select(lbsp => lbsp.Tag.ReadMetadata<scenario_lightmap_bsp_data>())
                    .FirstOrDefault(lbsp => lbsp.BspIndex == bspIndex)
                    ?? Cache.TagIndex.FirstOrDefault(t => t.ClassCode == "Lbsp" && t.TagName == Item.TagName)?.ReadMetadata<scenario_lightmap_bsp_data>();

            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo3Common.GetMaterials(Shaders));

            var clusterRegion = new GeometryRegion { Name = BlamConstants.SbspClustersGroupName };
            clusterRegion.Permutations.AddRange(
                Clusters.Select((c, i) => new GeometryPermutation
                {
                    SourceIndex = i,
                    Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                    MeshIndex = c.SectionIndex,
                    MeshCount = 1
                })
            );
            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => i.Name))
            {
                var sectionRegion = new GeometryRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i => new GeometryPermutation
                    {
                        SourceIndex = GeometryInstances.IndexOf(i),
                        Name = i.Name,
                        Transform = i.Transform,
                        TransformScale = i.TransformScale,
                        MeshIndex = i.SectionIndex,
                        MeshCount = 1
                    })
                );

                model.Regions.Add(sectionRegion);
            }

            model.Meshes.AddRange(Halo3Common.GetMeshes(Cache, lightmapData.ResourcePointer, lightmapData.Sections, (s, m) =>
            {
                var index = (short)lightmapData.Sections.IndexOf(s);
                m.BoundsIndex = index >= BoundingBoxes.Count ? null : index;
                m.IsInstancing = index < BoundingBoxes.Count;
            }));

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

    public partial class BspBoundingBoxBlock : IRealBounds5D
    {
        public RealBounds XBounds { get; set; }
        public RealBounds YBounds { get; set; }
        public RealBounds ZBounds { get; set; }
        public RealBounds UBounds { get; set; }
        public RealBounds VBounds { get; set; }
    }
}
