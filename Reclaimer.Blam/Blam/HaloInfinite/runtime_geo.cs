﻿using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Drawing;
using System.Numerics;

namespace Reclaimer.Blam.HaloInfinite
{
    public class runtime_geo : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public runtime_geo(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(16)]
        public BlockCollection<RuntimeGeoPerMeshData> PerMeshData { get; set; }

        [Offset(64)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(104)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(124)]
        public BlockCollection<NodeMapBlock> NodeMaps { get; set; }

        [Offset(186)]
        public ResourcePackingPolicy MeshResourcePackingPolicy { get; set; }

        [Offset(188)]
        public short TotalIndexBufferCount { get; set; }

        [Offset(190)]
        public short TotalVertexBufferCount { get; set; }

        [Offset(196)]
        public BlockCollection<MeshResourceGroupBlock> MeshResourceGroups { get; set; }

        [Offset(376)]
        public BlockCollection<MarkerGroupBlock> MarkerGroups { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        public Model GetModelContent()
        {
            var geoParams = new HaloInfiniteGeometryArgs
            {
                Module = Module,
                ResourcePolicy = MeshResourcePackingPolicy,
                Sections = Sections,
                NodeMaps = NodeMaps,
                MeshResourceGroups = MeshResourceGroups,
                ResourceIndex = Item.ResourceIndex,
                ResourceCount = Item.ResourceCount
            };

            var model = new Model { Name = Item.FileName, OriginalPath = Item.TagName };
            model.CustomProperties.Add(BlamConstants.SourceTagPropertyName, Item.TagName);

            model.Markers.AddRange(MarkerGroups.Select(g =>
            {
                var marker = new Marker { Name = g.Name };
                marker.Instances.AddRange(g.Markers.Select(m => new MarkerInstance
                {
                    Position = (Vector3)m.Position,
                    Rotation = new Quaternion(m.Rotation.X, m.Rotation.Y, m.Rotation.Z, m.Rotation.W),
                    RegionIndex = m.RegionIndex,
                    PermutationIndex = m.PermutationIndex,
                    BoneIndex = m.NodeIndex
                }));

                return marker;
            }));

            model.Meshes.AddRange(HaloInfiniteCommon.GetMeshes(geoParams, out var materials));
            
            // Hack: RTGO does not contain any regions, so i map all meshes to a single "default" region
            // with each mesh having its own permutation.
            
            var region = new ModelRegion { Name = "default" };
            for (int i = 0; i < PerMeshData.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh == null)
                    continue;

                var permutation = new ModelPermutation
                {
                    Name = PerMeshData[i].Name.ToString(),
                    MeshRange = (i, 1)
                };

                region.Permutations.Add(permutation);
            }
            model.Regions.Add(region);

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            return model;
        }

        #endregion
    }

    [FixedSize(144)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class RuntimeGeoPerMeshData
    {
        [Offset(0)]
        public StringHash Name { get; set; }

        [Offset(4)]
        public int MeshIndex { get; set; }

        [Offset(8)]
        public RealVector3 Scale { get; set; }

        [Offset(20)]
        public RealVector3 Position { get; set; }

        [Offset(108)]
        public BlockCollection<float> LodLevels { get; set; }
    }
}
