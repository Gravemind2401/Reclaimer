using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo4
{
    public class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        private bool loadedInstances;

        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        [Offset(268)]
        public RealBounds XBounds { get; set; }

        [Offset(276)]
        public RealBounds YBounds { get; set; }

        [Offset(284)]
        public RealBounds ZBounds { get; set; }

        [Offset(340)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(352)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(604)]
        public BlockCollection<BspGeometryInstanceGroupBlock> GeometryInstanceGroups { get; set; }

        [Offset(616)]
        public BlockCollection<BspGeometryInstanceSubGroupBlock> GeometryInstanceSubGroups { get; set; }

        [Offset(640)]
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(1040, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(844, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer1 { get; set; }

        [Offset(1440, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1048, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1072, MinVersion = (int)CacheType.MccHalo4)]
        public ResourceIdentifier ResourcePointer2 { get; set; }

        [Offset(1756, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1144, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1192, MinVersion = (int)CacheType.MccHalo4)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(932, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1168, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1216, MinVersion = (int)CacheType.MccHalo4)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(1888, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1288, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1336, MinVersion = (int)CacheType.MccHalo4)]
        public ResourceIdentifier ResourcePointer3 { get; set; }

        [Offset(1964, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1364, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1436, MinVersion = (int)CacheType.MccHalo4)]
        public ResourceIdentifier InstancesResourcePointer { get; set; } //datum 5

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent());

        private Model GetModelContent()
        {
            var scenario = Cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var model = new Model { Name = Item.FileName };

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == Item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = lightmap.LightmapRefs[bspIndex].LightmapDataReference.Tag.ReadMetadata<scenario_lightmap_bsp_data>();

            var geoParams = new Halo4GeometryArgs
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

            if (!loadedInstances)
            {
                var resourceGestalt = Cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
                var entry = resourceGestalt.ResourceEntries[InstancesResourcePointer.ResourceIndex];
                var address = entry.ResourceFixups[entry.ResourceFixups.Count - 10].Offset & 0x0FFFFFFF;

                using (var ms = new MemoryStream(InstancesResourcePointer.ReadData(PageType.Auto)))
                using (var reader = new EndianReader(ms, Cache.ByteOrder))
                {
                    var blockSize = Cache.CacheType == CacheType.Halo4Beta ? 164 : 148;
                    for (var i = 0; i < GeometryInstances.Count; i++)
                    {
                        reader.Seek(address + blockSize * i, SeekOrigin.Begin);
                        GeometryInstances[i].TransformScale = reader.ReadSingle();
                        GeometryInstances[i].Transform = reader.ReadMatrix3x4();
                        reader.Seek(10, SeekOrigin.Current);
                        GeometryInstances[i].SectionIndex = reader.ReadInt16();
                    }
                }

                loadedInstances = true;
            }

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

            model.Meshes.AddRange(Halo4Common.GetMeshes(geoParams, out _));
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

    [FixedSize(140, MaxVersion = (int)CacheType.MccHalo2XU11)]
    [FixedSize(128, MinVersion = (int)CacheType.MccHalo2XU11)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(64, MaxVersion = (int)CacheType.MccHalo2XU11)]
        [Offset(52, MinVersion = (int)CacheType.MccHalo2XU11)]
        public short SectionIndex { get; set; }
    }

    [FixedSize(16)]
    public class BspGeometryInstanceGroupBlock
    {
        [Offset(4)]
        public BlockCollection<short> SubGroupIndexes { get; set; }
    }

    [FixedSize(44)]
    public class BspGeometryInstanceSubGroupBlock
    {
        [Offset(32)]
        public BlockCollection<short> InstanceIndexes { get; set; }
    }

    [FixedSize(4)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class BspGeometryInstanceBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        //these are not stored in the metadata
        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public short SectionIndex { get; set; }
    }
}
