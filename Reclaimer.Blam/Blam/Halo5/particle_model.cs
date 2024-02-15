using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Globalization;
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


        public override string ToString() => Item.FileName;

        #region IContentProvider

        Model IContentProvider<Model>.GetContent() => GetModelContent();

        public override Scene GetContent() => Scene.WrapSingleModel(GetModelContent(), BlamConstants.Gen3UnitScale);

        private Model GetModelContent()
        {
            var geoParams = new Halo5GeometryArgs
            {
                Module = Module,
                ResourcePolicy = ResourcePackingPolicy.SingleResource,
                //Regions = Regions,
                Materials = null, // Materials,
                Sections = Sections,
                //NodeMaps = NodeMaps,
                ResourceIndex = Item.ResourceIndex,
                ResourceCount = Item.ResourceCount
            };

            var model = new Model { Name = Item.FileName };

            var sectionRegion = new ModelRegion { Name = "Placeholder" };
            sectionRegion.Permutations.AddRange(
                new List<ModelPermutation> {new ModelPermutation
                {
                    Name = "Placeholder_pmdf",

                    Transform = new Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        0, 0, 0, 1
                    ),
                    Scale = new Vector3(1, 1, 1),

                    MeshRange = (0, 1),
                    IsInstanced = false
                }}
            );
            model.Regions.Add(sectionRegion);
            


            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));

            var bounds = BoundingBoxes[0];
            var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
            var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);
            model.SetCompressionBounds(posBounds, texBounds);

            return model;
        }


        #endregion
    }
}
