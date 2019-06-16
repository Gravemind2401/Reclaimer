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

namespace Adjutant.Blam.Halo3
{
    public class scenario_structure_bsp : IRenderGeometry
    {
        private readonly CacheFile cache;
        private readonly IndexItem item;

        public scenario_structure_bsp(CacheFile cache, IndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(60, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(64, MinVersion = (int)CacheType.Halo3ODST)]
        public RealBounds XBounds { get; set; }

        [Offset(68, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(72, MinVersion = (int)CacheType.Halo3ODST)]
        public RealBounds YBounds { get; set; }

        [Offset(76, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(80, MinVersion = (int)CacheType.Halo3ODST)]
        public RealBounds ZBounds { get; set; }

        [Offset(180, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(184, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(192, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(196, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<ShaderBlock> Shaders { get; set; }

        [Offset(432, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(436, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(580, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(584, MinVersion = (int)CacheType.Halo3ODST)]
        public ResourceIdentifier ResourcePointer1 { get; set; }

        [Offset(740, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(744, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(752, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(756, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<BspBoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(860, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(864, MinVersion = (int)CacheType.Halo3ODST)]
        public ResourceIdentifier ResourcePointer2 { get; set; }

        [Offset(892, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(896, MinVersion = (int)CacheType.Halo3ODST)]
        public ResourceIdentifier ResourcePointer3 { get; set; }

        #region IRenderGeometry

        int IRenderGeometry.LodCount => 1;

        public IGeometryModel ReadGeometry(int lod)
        {
            if (lod < 0 || lod >= ((IRenderGeometry)this).LodCount)
                throw new ArgumentOutOfRangeException(nameof(lod));

            var model = new GeometryModel(Path.GetFileName(item.FileName)) { CoordinateSystem = CoordinateSystem.Default };

            var bspBlock = cache.Scenario.StructureBsps.First(s => s.BspReference.TagId == item.Id);
            var bspIndex = cache.Scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = cache.Scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = cache.CacheType < CacheType.Halo3ODST
                ? lightmap.LightmapData[bspIndex]
                : lightmap.LightmapRefs[bspIndex].Tag.ReadMetadata<scenario_lightmap_bsp_data>();

            model.Bounds.AddRange(BoundingBoxes);
            model.Materials.AddRange(Halo3Common.GetMaterials(Shaders));

            var clusterRegion = new GeometryRegion { Name = "Clusters" };
            clusterRegion.Permutations.AddRange(
                Clusters.Select(c => new GeometryPermutation
                {
                    Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                    NodeIndex = byte.MaxValue,
                    Transform = Matrix4x4.Identity,
                    TransformScale = 1,
                    MeshIndex = c.SectionIndex,
                    MeshCount = 1
                })
            );
            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in GeometryInstances.GroupBy(i => i.SectionIndex))
            {
                var section = lightmapData.Sections[instanceGroup.Key];
                var sectionRegion = new GeometryRegion { Name = Utils.CurrentCulture($"Instances {instanceGroup.Key:D3}") };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i => new GeometryPermutation
                    {
                        Name = i.Name,
                        NodeIndex = byte.MaxValue,
                        Transform = i.Transform,
                        TransformScale = i.TransformScale,
                        MeshIndex = i.SectionIndex,
                        MeshCount = 1
                    })
                );
                model.Regions.Add(sectionRegion);
            }

            model.Meshes.AddRange(Halo3Common.GetMeshes(cache, lightmapData.ResourcePointer, lightmapData.Sections, (s) =>
            {
                var index = lightmapData.Sections.IndexOf(s);
                return (short)(index >= BoundingBoxes.Count ? -1 : index);
            }));

            return model;
        }

        #endregion

    }

    [FixedSize(236, MaxVersion = (int)CacheType.Halo3Retail)]
    [FixedSize(220, MinVersion = (int)CacheType.Halo3Retail)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(172, MaxVersion = (int)CacheType.Halo3Retail)]
        [Offset(156, MinVersion = (int)CacheType.Halo3Retail)]
        public short SectionIndex { get; set; }
    }

    [FixedSize(120)]
    public class BspGeometryInstanceBlock
    {
        [Offset(0)]
        public float TransformScale { get; set; }

        [Offset(4)]
        public Matrix4x4 Transform { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }

        [Offset(84)]
        public StringId Name { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(44)]
    public class BspBoundingBoxBlock : IRealBounds5D
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

        #region IRealBounds5D

        IRealBounds IRealBounds5D.XBounds => XBounds;

        IRealBounds IRealBounds5D.YBounds => YBounds;

        IRealBounds IRealBounds5D.ZBounds => ZBounds;

        IRealBounds IRealBounds5D.UBounds => UBounds;

        IRealBounds IRealBounds5D.VBounds => VBounds;

        #endregion
    }
}
