using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.IO;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public partial class structure_lightmap : ContentTagDefinition<Scene>, IContentProvider<Model>
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
                //ModuleItem = Item,
                ResourcePolicy = ResourcePackingPolicy.SingleResource, // MeshResourcePackingPolicy, // we dont have the exact same block as the model, but i think we do have it in the render geo struct
                //Regions = Regions,
                Materials = Materials,
                Sections = Sections,
                //NodeMaps = NodeMaps,
                ResourceIndex = Item.ResourceIndex+1, // it looks like we use the second resource file to access the geo data
                ResourceCount = Item.ResourceCount-1
            };

            var model = new Model { Name = Item.FileName };


            var clusterRegion = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };
            //clusterRegion.Permutations.AddRange(
            //    Clusters.Select((c, i) => new ModelPermutation
            //    {
            //        Name = Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
            //        MeshRange = (c.SectionIndex, 1)
            //    })
            //);
            model.Regions.Add(clusterRegion);
            // whoops i thought 'i' was index, cant be bothered fixing it though
            foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(GeometryInstances, i => "name_placeholder" + i))
            {
                var sectionRegion = new ModelRegion { Name = instanceGroup.Key };
                sectionRegion.Permutations.AddRange(
                    instanceGroup.Select(i => new ModelPermutation
                    {
                        Name = "Placeholder" + i,

                        Transform = new Matrix4x4(
                            1, 0, 0, 0,
                            0, 1, 0, 0,
                            0, 0, 1, 0,
                            0, 0, 0, 1
                        ),
                        Scale = new Vector3(1, 1, 1),

                        MeshRange = (i.MeshIndex, 1),
                        IsInstanced = true
                    })
                );
                model.Regions.Add(sectionRegion);
            }

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
    //[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    public partial class StructureLightmapInstanceBlock
    {
        [Offset(88)]
        public short MeshIndex { get; set; }
        [Offset(90)]
        public short BoundsIndex { get; set; }
    }
}
