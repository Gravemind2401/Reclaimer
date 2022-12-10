using Adjutant.Geometry;
using Adjutant.Spatial;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.HaloReach
{
    public class scenario_structure_bsp : IRenderGeometry
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        private bool loadedInstances;

        public scenario_structure_bsp(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(236, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(240, MinVersion = (int)CacheType.MccHaloReach)]
        public RealBounds XBounds { get; set; }

        [Offset(244, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(248, MinVersion = (int)CacheType.MccHaloReach)]
        public RealBounds YBounds { get; set; }

        [Offset(252, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(256, MinVersion = (int)CacheType.MccHaloReach)]
        public RealBounds ZBounds { get; set; }

        [Offset(308, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(312, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(320, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(324, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(620, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(608, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(612, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(1112, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(1100, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(1128, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(1124, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(1112, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(1140, MinVersion = (int)CacheType.MccHaloReach)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [MinVersion((int)CacheType.HaloReachRetail)]
        [Offset(1296, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(1336, MinVersion = (int)CacheType.MccHaloReach)]
        public ResourceIdentifier InstancesResourcePointer { get; set; }

        #region IRenderGeometry

        string IRenderGeometry.SourceFile => item.CacheFile.FileName;

        int IRenderGeometry.Id => item.Id;

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var scenario = cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var model = new GeometryModel(item.FileName) { CoordinateSystem = CoordinateSystem.Default };

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = lightmap.LightmapRefs[bspIndex].LightmapDataReference.Tag.ReadMetadata<scenario_lightmap_bsp_data>();

            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(HaloReachCommon.GetMaterials(Shaders));

            var clusterRegion = new GeometryRegion { Name = "Clusters" };
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

            if (cache.CacheType >= CacheType.HaloReachRetail && !loadedInstances)
            {
                var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
                var entry = resourceGestalt.ResourceEntries[InstancesResourcePointer.ResourceIndex];
                var address = entry.FixupOffset + entry.ResourceFixups[entry.ResourceFixups.Count - 10].Offset & 0x0FFFFFFF;

                using (var cacheReader = cache.CreateReader(cache.DefaultAddressTranslator))
                using (var reader = cacheReader.CreateVirtualReader(resourceGestalt.FixupDataPointer.Address))
                {
                    for (int i = 0; i < GeometryInstances.Count; i++)
                    {
                        reader.Seek(address + 156 * i, SeekOrigin.Begin);
                        GeometryInstances[i].TransformScale = reader.ReadSingle();
                        GeometryInstances[i].Transform = reader.ReadObject<Matrix4x4>();
                        reader.Seek(6, SeekOrigin.Current);
                        GeometryInstances[i].SectionIndex = reader.ReadInt16();
                    }
                }

                loadedInstances = true;
            }

            foreach (var instanceGroup in GeometryInstances.GroupBy(i => i.SectionIndex))
            {
                var section = lightmapData.Sections[instanceGroup.Key];
                var sectionRegion = new GeometryRegion { Name = Utils.CurrentCulture($"Instances {instanceGroup.Key:D3}") };
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

            model.Meshes.AddRange(HaloReachCommon.GetMeshes(cache, lightmapData.ResourcePointer, lightmapData.Sections, (s, m) =>
            {
                var index = (short)lightmapData.Sections.IndexOf(s);
                m.BoundsIndex = index >= BoundingBoxes.Count ? (short?)null : index;
                m.IsInstancing = index < BoundingBoxes.Count;
            }));

            return model;
        }

        public IEnumerable<IBitmap> GetAllBitmaps() => HaloReachCommon.GetBitmaps(Shaders);

        public IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes) => HaloReachCommon.GetBitmaps(Shaders, shaderIndexes);

        #endregion
    }

    [FixedSize(288, MaxVersion = (int)CacheType.HaloReachRetail)]
    [FixedSize(140, MinVersion = (int)CacheType.HaloReachRetail)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(208, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(64, MinVersion = (int)CacheType.HaloReachRetail)]
        public short SectionIndex { get; set; }
    }

    [FixedSize(168, MaxVersion = (int)CacheType.HaloReachRetail)]
    [FixedSize(4, MinVersion = (int)CacheType.HaloReachRetail)]
    public class BspGeometryInstanceBlock
    {
        [Offset(0)]
        [VersionSpecific((int)CacheType.HaloReachBeta)]
        public float TransformScale { get; set; }

        [Offset(4)]
        [VersionSpecific((int)CacheType.HaloReachBeta)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        [VersionSpecific((int)CacheType.HaloReachBeta)]
        public short SectionIndex { get; set; }

        [Offset(124, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(0, MinVersion = (int)CacheType.HaloReachRetail)]
        public StringId Name { get; set; }

        public override string ToString() => Name;
    }
}
