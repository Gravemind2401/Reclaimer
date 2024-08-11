using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.HaloReach
{
    public partial class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        private bool loadedInstances;

        public scenario_structure_bsp(IIndexItem item)
            : base(item)
        { }

        public RealBounds XBounds { get; set; }
        public RealBounds YBounds { get; set; }
        public RealBounds ZBounds { get; set; }
        public BlockCollection<ClusterBlock> Clusters { get; set; }
        public BlockCollection<ShaderBlock> Shaders { get; set; }
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }
        public BlockCollection<SectionBlock> Sections { get; set; }
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }
        public ResourceIdentifier InstancesResourcePointer { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            var scenario = Cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();

            var bspBlock = scenario.StructureBsps.First(s => s.BspReference.TagId == Item.Id);
            var bspIndex = scenario.StructureBsps.IndexOf(bspBlock);

            var lightmap = scenario.ScenarioLightmapReference.Tag.ReadMetadata<scenario_lightmap>();
            var lightmapData = lightmap.LightmapRefs[bspIndex].LightmapDataReference.Tag.ReadMetadata<scenario_lightmap_bsp_data>();

            var geoParams = new HaloReachGeometryArgs
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

            if (Cache.CacheType >= CacheType.HaloReachRetail && !loadedInstances)
            {
                var resourceGestalt = Cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
                var entry = resourceGestalt.ResourceEntries[InstancesResourcePointer.ResourceIndex];
                var address = entry.FixupOffset + entry.ResourceFixups[entry.ResourceFixups.Count - 10].Offset & 0x0FFFFFFF;

                using (var cacheReader = Cache.CreateReader(Cache.DefaultAddressTranslator))
                using (var reader = cacheReader.CreateVirtualReader(resourceGestalt.FixupDataPointer.Address))
                {
                    for (var i = 0; i < GeometryInstances.Count; i++)
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

            model.Meshes.AddRange(HaloReachCommon.GetMeshes(geoParams, out _));
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
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
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
    }
}
