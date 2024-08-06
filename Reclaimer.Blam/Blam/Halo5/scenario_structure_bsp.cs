using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public scenario_structure_bsp(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(280)]
        public RealBounds XBounds { get; set; }

        [Offset(288)]
        public RealBounds YBounds { get; set; }

        [Offset(296)]
        public RealBounds ZBounds { get; set; }

        [Offset(332)]
        public BlockCollection<ClusterBlock> Clusters { get; set; }

        [Offset(360)]
        public BlockCollection<MaterialBlock> Materials { get; set; }

        [Offset(700)]
        public BlockCollection<BspGeometryInstanceBlock> GeometryInstances { get; set; }

        [Offset(912)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(968)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent());

        private Model GetModelContent()
        {
            var geoParams = new Halo5GeometryArgs
            {
                Module = Module,
                ResourcePolicy = ResourcePackingPolicy.SingleResource, // MeshResourcePackingPolicy, // we dont have the exact same block as the model, but i think we do have it in the render geo struct
                Materials = Materials,
                Sections = Sections,
                ResourceIndex = Item.ResourceIndex,
                ResourceCount = Item.ResourceCount
            };

            var model = new Model { Name = Item.FileName };

            var clusterRegion = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };
            clusterRegion.Permutations.AddRange(
                Clusters.Select((c, i) => new ModelPermutation
                {
                    Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                    MeshRange = (c.SectionIndex, 1)
                })
            );

            model.Regions.Add(clusterRegion);

            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => i.Name.Value))
            {
                var sectionRegion = new ModelRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i => new ModelPermutation
                    {
                        Name = i.Name,
                        Transform = i.Transform,
                        Scale = (Vector3)i.TransformScale,
                        MeshRange = (i.MeshIndex, 1),
                        IsInstanced = true
                    })
                );
                model.Regions.Add(sectionRegion);
            }

            // 1 we need to use the correct mesh indices

            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));

            // i changed the old system of getting bounding boxes by using the corresponding mesh index to explicitly use the index provided by the geometry instance
            foreach (var instanceGroup in GeometryInstances)
            {
                if (model.Meshes[instanceGroup.MeshIndex] == null)
                    continue;

                var bounds = BoundingBoxes[instanceGroup.BoundsIndex];
                var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);

                (model.Meshes[instanceGroup.MeshIndex].PositionBounds, model.Meshes[instanceGroup.MeshIndex].TextureBounds) = (posBounds, texBounds);
            }

            return model;
        }

        #endregion
    }

    [FixedSize(84)]
    public class ClusterBlock
    {
        [Offset(0)]
        public RealBounds XBounds { get; set; }

        [Offset(8)]
        public RealBounds YBounds { get; set; }

        [Offset(16)]
        public RealBounds ZBounds { get; set; }

        [Offset(52)]
        public short SectionIndex { get; set; }
    }

    [FixedSize(528)]
    [DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public class BspGeometryInstanceBlock
    {
        [Offset(0)]
        public RealVector3 TransformScale { get; set; }

        [Offset(12)]
        public Matrix4x4 Transform { get; set; }

        [Offset(60)]
        public short SectionIndex { get; set; }

        [Offset(64)]
        public short MeshIndex { get; set; }

        [Offset(66)]
        public short BoundsIndex { get; set; }

        [Offset(268)]
        public StringHash Name { get; set; }
    }
}
