using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public class particle_model : ContentTagDefinition<Scene>, IContentProvider<Model>
    {
        public particle_model(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(48)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(104)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.WorldUnitScale);

        private Model GetModelContent()
        {
            var geoParams = new Halo5GeometryArgs
            {
                Module = Module,
                ResourcePolicy = ResourcePackingPolicy.SingleResource,
                Materials = null,
                Sections = Sections,
                ResourceIndex = Item.ResourceIndex,
                ResourceCount = Item.ResourceCount
            };

            var model = new Model { Name = Item.FileName };

            var region = new ModelRegion { Name = "particle_model" };
            region.Permutations.Add(
                new ModelPermutation
                {
                    Name = "default",
                    MeshRange = (0, 1)
                }
            );

            model.Regions.Add(region);
            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out _));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            return model;
        }

        #endregion
    }
}
