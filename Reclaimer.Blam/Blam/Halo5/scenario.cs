using Reclaimer.Blam.Common;
using Reclaimer.Blam.Halo5;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Geometry;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.IO;
using System.Numerics;


namespace Reclaimer.Blam.Halo5
{
    public partial class scenario : ContentTagDefinition<Scene>, IContentProvider<Scene>
    {

        public scenario(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(380)]
        public BlockCollection<StructureBspsBlock> StructureBsps { get; set; }


        [Offset(808)]
        public BlockCollection<SceneryPlacementBlock> Scenery { get; set; }

        [Offset(836)]
        public BlockCollection<SceneryPaletteBlock> SceneryPalette { get; set; }

        public override Scene GetContent()
        {
            var scene = new Scene { Name = Item.FileName, CoordinateSystem = CoordinateSystem.Default.WithScale(BlamConstants.Gen3UnitScale) };
            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };
            var sceneryGroup = new SceneGroup { Name = BlamConstants.ScenarioSceneryGroupName };

            //TODO: display error models in some way

            foreach (var v in StructureBsps)
            {
                try
                {
                    var bspItem = v.StructureBsp.Tag;
                    var bspTag = bspItem.ReadMetadata<scenario_structure_bsp>();

                    ModuleItem stlmItem = null;
                    structure_lightmap stlmTag = null;
                    try
                    {
                        if (v.LightingVariants.Count > 0)
                        {
                            stlmItem = v.LightingVariants[0].StructureLightmap.Tag;
                            stlmTag = stlmItem.ReadMetadata<structure_lightmap>();
                        }
                    }
                    catch { stlmItem = null; stlmTag = null; }
                    // adjust parameters to use geo data from
                    Halo5GeometryArgs geoParams;
                    if (stlmTag == null)
                        geoParams = new Halo5GeometryArgs
                        {
                            Module = bspItem.Module,
                            ResourcePolicy = ResourcePackingPolicy.SingleResource, 
                            Materials = bspTag.Materials,
                            Sections = bspTag.Sections,
                            ResourceIndex = bspItem.ResourceIndex,
                            ResourceCount = bspItem.ResourceCount
                        };
                    else
                        geoParams = new Halo5GeometryArgs
                        {
                            Module = stlmItem.Module,
                            ResourcePolicy = ResourcePackingPolicy.SingleResource,
                            Materials = stlmTag.Materials,
                            Sections = stlmTag.Sections,
                            ResourceIndex = stlmItem.ResourceIndex+1, // second resource is the geometry resource
                            ResourceCount = stlmItem.ResourceCount-1
                        };

                    var model = new Model { Name = Item.FileName };


                    var clusterRegion = new ModelRegion { Name = BlamConstants.SbspClustersGroupName };
                    clusterRegion.Permutations.AddRange(
                        bspTag.Clusters.Select((c, i) => new ModelPermutation
                        {
                            Name = bspTag.Clusters.IndexOf(c).ToString("D3", CultureInfo.CurrentCulture),
                            MeshRange = (c.SectionIndex, 1)
                        })
                    );

                    model.Regions.Add(clusterRegion);

                    foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(bspTag.GeometryInstances, i => i.Name.Value))
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
                    foreach (var instanceGroup in bspTag.GeometryInstances)
                    {
                        if (model.Meshes[instanceGroup.MeshIndex] == null)
                            continue;

                        var bounds = bspTag.BoundingBoxes[instanceGroup.BoundsIndex];
                        var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                        var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);

                        (model.Meshes[instanceGroup.MeshIndex].PositionBounds, model.Meshes[instanceGroup.MeshIndex].TextureBounds) = (posBounds, texBounds);
                    }

                    // add model to scene
                    model.Flags |= SceneFlags.PrimaryFocus;
                    bspGroup.ChildObjects.Add(model);
                }
                catch { }
            }


            foreach (var group in Scenery.EmptyIfNull().GroupBy(x => x.PaletteIndex))
            {
                var tagRef = SceneryPalette?.ElementAtOrDefault(group.Key)?.TagReference;
                if (!tagRef.HasValue)
                    continue;

                Model model = null;
                try
                {
                    var tag = tagRef.Value.Tag.ReadMetadata<scenery>();
                    model = (tag.GetModel() as IContentProvider<Model>)?.GetContent();
                }
                catch { }

                if (model == null)
                    continue;

                var placementGroup = new SceneGroup { Name = model.Name };

                foreach (var placement in group)
                {
                    try
                    {
                        var sceneItem = new ObjectPlacement(model);
                        sceneItem.SetTransform(placement.Scale, placement.Position.ToVector3(), (Quaternion)(EulerAngles)placement.Rotation);
                        placementGroup.ChildObjects.Add(sceneItem);
                    }
                    catch { }
                }

                if (placementGroup.HasItems)
                    sceneryGroup.ChildGroups.Add(placementGroup);
            }


            if (bspGroup.HasItems)
                scene.ChildGroups.Add(bspGroup);

            if (sceneryGroup.HasItems)
                scene.ChildGroups.Add(sceneryGroup);

            return scene;
        }
    }

    [FixedSize(196)]
    public partial class StructureBspsBlock
    {
        [Offset(0)]
        public TagReference StructureBsp { get; set; }
        [Offset(140)]
        public BlockCollection<StructureLighting> LightingVariants { get; set; }
    }
    [FixedSize(140)]
    public partial class StructureLighting
    {
        [Offset(36)]
        public TagReference StructureLightmap { get; set; }
    }





    [FixedSize(720)]
    public partial class SceneryPlacementBlock
    {
        [Offset(0)]
        public short PaletteIndex { get; set; }
        [Offset(2)]
        public short NameIndex { get; set; }
        [Offset(12)]
        public RealVector3 Position { get; set; }
        [Offset(24)]
        public RealVector3 Rotation { get; set; }
        [Offset(36)]
        public float Scale { get; set; }
    }

    [FixedSize(32)]
    [DebuggerDisplay($"{{{nameof(TagReference)},nq}}")]
    public partial class SceneryPaletteBlock
    {
        [Offset(0)]
        public TagReference TagReference { get; set; }
    }
}
