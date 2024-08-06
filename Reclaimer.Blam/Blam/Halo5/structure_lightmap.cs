using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public class structure_lightmap : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public structure_lightmap(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(556)]
        public BlockCollection<MaterialBlock> Materials { get; set; }

        [Offset(332)]
        public BlockCollection<StructureLightmapInstanceBlock> GeometryInstances { get; set; }

        [Offset(616)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(672)]
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
                ResourceIndex = Item.ResourceIndex + 1, // it looks like we use the second resource file to access the geo data
                ResourceCount = Item.ResourceCount - 1
            };

            var model = new Model { Name = Item.FileName };

            var region = new ModelRegion { Name = BlamConstants.ModelInstancesGroupName };
            region.Permutations.AddRange(GeometryInstances.Select((instance, index) =>
                new ModelPermutation
                {
                    Name = index.ToString("D3"),
                    Transform = Matrix4x4.Identity,
                    Scale = Vector3.One,
                    MeshRange = (instance.MeshIndex, 1),
                    IsInstanced = true
                })
            );

            model.Regions.Add(region);

            // 1 we need to use the correct mesh indices

            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));
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

    [FixedSize(96)]
    public class StructureLightmapInstanceBlock
    {
        [Offset(88)]
        public short MeshIndex { get; set; }

        [Offset(90)]
        public short BoundsIndex { get; set; }
    }
}
