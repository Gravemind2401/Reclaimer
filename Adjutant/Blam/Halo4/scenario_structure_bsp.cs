using Adjutant.Blam.Common;
using Adjutant.Geometry;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
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
        [Offset(1048, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer2 { get; set; }

        [Offset(1756, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1144, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(932, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1168, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(1888, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1288, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer3 { get; set; }

        [Offset(1964, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1364, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier InstancesResourcePointer { get; set; }

        #region IRenderGeometry

        string IRenderGeometry.Name => item.FullPath;

        string IRenderGeometry.Class => item.ClassName;

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var scenario = cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var model = new GeometryModel(item.FileName()) { CoordinateSystem = CoordinateSystem.Default };

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = lightmap.LightmapRefs[bspIndex].LightmapDataReference.Tag.ReadMetadata<scenario_lightmap_bsp_data>();

            model.Bounds.AddRange(lightmapData.BoundingBoxes);
            model.Materials.AddRange(Halo4Common.GetMaterials(Shaders));

            var clusterRegion = new GeometryRegion { Name = "Clusters" };
            clusterRegion.Permutations.AddRange(
                Clusters.Select(c => new GeometryPermutation
                {
                    Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                    MeshIndex = c.SectionIndex,
                    MeshCount = 1
                })
            );
            model.Regions.Add(clusterRegion);

            if (!loadedInstances)
            {
                var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
                var entry = resourceGestalt.ResourceEntries[InstancesResourcePointer.ResourceIndex];
                var address = entry.ResourceFixups[entry.ResourceFixups.Count - 10].Offset & 0x0FFFFFFF;

                using (var ms = new MemoryStream(InstancesResourcePointer.ReadData(PageType.Auto)))
                using (var reader = new EndianReader(ms, cache.ByteOrder))
                {
                    var blockSize = cache.CacheType == CacheType.Halo4Beta ? 164 : 148;
                    for (int i = 0; i < GeometryInstances.Count; i++)
                    {
                        reader.Seek(address + blockSize * i, SeekOrigin.Begin);
                        GeometryInstances[i].TransformScale = reader.ReadSingle();
                        GeometryInstances[i].Transform = new Matrix4x4
                        {
                            M11 = reader.ReadSingle(),
                            M12 = reader.ReadSingle(),
                            M13 = reader.ReadSingle(),

                            M21 = reader.ReadSingle(),
                            M22 = reader.ReadSingle(),
                            M23 = reader.ReadSingle(),

                            M31 = reader.ReadSingle(),
                            M32 = reader.ReadSingle(),
                            M33 = reader.ReadSingle(),

                            M41 = reader.ReadSingle(),
                            M42 = reader.ReadSingle(),
                            M43 = reader.ReadSingle(),
                        };
                        reader.Seek(10, SeekOrigin.Current);
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
                        Name = i.Name,
                        Transform = i.Transform,
                        TransformScale = i.TransformScale,
                        MeshIndex = i.SectionIndex,
                        MeshCount = 1
                    })
                );
                model.Regions.Add(sectionRegion);
            }

            model.Meshes.AddRange(Halo4Common.GetMeshes(cache, lightmapData.ResourcePointer, lightmapData.Sections, (s, m) =>
            {
                var index = (short)lightmapData.Sections.IndexOf(s);
                m.BoundsIndex = index >= lightmapData.BoundingBoxes.Count ? (short?)null : index;
                m.IsInstancing = index < lightmapData.BoundingBoxes.Count;
            }));

            return model;
        }

        public IEnumerable<IBitmap> GetAllBitmaps()
        {
            var complete = new List<int>();

            foreach (var s in Shaders)
            {
                var rmsh = s.MaterialReference.Tag?.ReadMetadata<material>();
                if (rmsh == null) continue;

                foreach (var map in rmsh.ShaderProperties.SelectMany(p => p.ShaderMaps))
                {
                    if (map.BitmapReference.Tag == null || complete.Contains(map.BitmapReference.TagId))
                        continue;

                    complete.Add(map.BitmapReference.TagId);
                    yield return map.BitmapReference.Tag.ReadMetadata<bitmap>();
                }
            }
        }

        #endregion
    }

    [FixedSize(140)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(64)]
        public short SectionIndex { get; set; }
    }

    [FixedSize(16)]
    public class BspGeometryInstanceGroupBlock
    {
        [Offset(4)]
        public BlockCollection<short> SunGroupIndexes { get; set; }
    }

    [FixedSize(44)]
    public class BspGeometryInstanceSubGroupBlock
    {
        [Offset(32)]
        public BlockCollection<short> InstanceIndexes { get; set; }
    }

    [FixedSize(4)]
    public class BspGeometryInstanceBlock
    {
        [Offset(0)]
        public StringId Name { get; set; }

        //these are not stored in the metadata
        public float TransformScale { get; set; }
        public Matrix4x4 Transform { get; set; }
        public short SectionIndex { get; set; }

        public override string ToString() => Name;
    }
}
