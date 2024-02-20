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
    public partial class scenario_structure_bsp : ContentTagDefinition<Scene>, IContentProvider<Model>
    {

        public scenario_structure_bsp(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }


        //[Offset(24)] // sbsp dont got this?
        //public ResourcePackingPolicy MeshResourcePackingPolicy { get; set; }

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
                //ModuleItem = Item,
                ResourcePolicy = ResourcePackingPolicy.SingleResource, // MeshResourcePackingPolicy, // we dont have the exact same block as the model, but i think we do have it in the render geo struct
                //Regions = Regions,
                Materials = Materials,
                Sections = Sections,
                //NodeMaps = NodeMaps,
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

                        Transform = new Matrix4x4(
                            i.forward1, i.forward2, i.forward3, 0,
                            i.left1, i.left2, i.left3, 0,
                            i.up1, i.up2, i.up3, 0,
                            i.pos1, i.pos2, i.pos3, 1
                        ),
                        Scale = new Vector3(i.scale1, i.scale2, i.scale3),

                        MeshRange = (i.MeshIndex, 1),
                        IsInstanced = true
                    })
                );
                model.Regions.Add(sectionRegion);
            }

            // 1 we need to use the correct mesh indices

            model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));
            //foreach (var i in Enumerable.Range(0, BoundingBoxes.Count))
            //{
            //}
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
    public partial class ClusterBlock
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
    public partial class BspGeometryInstanceBlock
    {
        //[Offset(0)]
        //public Vector3 TransformScale { get; set; }


        //[Offset(12)]
        //public Vector3 TransformForward { get; set; }
        //[Offset(24)]
        //public Vector3 TransformLeft { get; set; }
        //[Offset(36)]
        //public Vector3 TransformUp { get; set; }
        //[Offset(48)]
        //public Vector3 TransformPosition { get; set; }


        [Offset(0)] public float scale1 { get; set; }
        [Offset(4)] public float scale2 { get; set; }
        [Offset(8)] public float scale3 { get; set; }

        [Offset(12)] public float forward1 { get; set; }
        [Offset(16)] public float forward2 { get; set; }
        [Offset(20)] public float forward3 { get; set; }

        [Offset(24)] public float left1 { get; set; }
        [Offset(28)] public float left2 { get; set; }
        [Offset(32)] public float left3 { get; set; }

        [Offset(36)] public float up1 { get; set; }
        [Offset(40)] public float up2 { get; set; }
        [Offset(44)] public float up3 { get; set; }

        [Offset(48)] public float pos1 { get; set; }
        [Offset(52)] public float pos2 { get; set; }
        [Offset(56)] public float pos3 { get; set; }


        [Offset(60)]
        public short SectionIndex { get; set; }
        [Offset(64)]
        public short MeshIndex { get; set; }
        [Offset(66)]
        public short BoundsIndex { get; set; }

        [Offset(268)]
        public StringHash Name { get; set; }
    }

    //[FixedSize(140)]
    //public partial class BspBoundingBoxBlock
    //{
    //    [Offset(4)]
    //    public RealBounds XBounds { get; set; }

    //    [Offset(12)]
    //    public RealBounds YBounds { get; set; }

    //    [Offset(20)]
    //    public RealBounds ZBounds { get; set; }

    //    [Offset(28)]
    //    public RealBounds UBounds { get; set; }

    //    [Offset(36)]
    //    public RealBounds VBounds { get; set; }
    //}





    //[FixedSize(4)]
    //[DebuggerDisplay($"{{{nameof(Name)},nq}}")]
    //public class BspGeometryInstanceBlock
    //{
    //    [Offset(0)]
    //    public StringId Name { get; set; }

    //    //these are not stored in the metadata
    //    public float TransformScale { get; set; }
    //    public Matrix4x4 Transform { get; set; }
    //    public short SectionIndex { get; set; }
    //}




}
