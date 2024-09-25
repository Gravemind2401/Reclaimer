using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public partial class scenario : ContentTagDefinition<Scene>, IContentProvider<Scene>
    {
        public scenario(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public BlockCollection<ObjectNameBlock> ObjectNames { get; set; }
        public BlockCollection<SkyReferenceBlock> Skies { get; set; }
        public BlockCollection<SceneryPlacementBlock> Scenery { get; set; }
        public BlockCollection<ObjectPaletteBlock> SceneryPalette { get; set; }
        public BlockCollection<MachinePlacementBlock> Machines { get; set; }
        public BlockCollection<ObjectPaletteBlock> MachinePalette { get; set; }
        public BlockCollection<ControlPlacementBlock> Controls { get; set; }
        public BlockCollection<ObjectPaletteBlock> ControlPalette { get; set; }
        public BlockCollection<CratePlacementBlock> Crates { get; set; }
        public BlockCollection<ObjectPaletteBlock> CratePalette { get; set; }

        public override Scene GetContent()
        {
            var scene = new Scene
            {
                Name = Item.FileName,
                OriginalPath = Item.TagName,
                CoordinateSystem = CoordinateSystem.Default.WithScale(BlamConstants.WorldUnitScale)
            };

            //TODO: display error models in some way

            #region StructureBsps

            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };

            foreach (var bspBlock in StructureBsps)
            {
                try
                {
                    var bspItem = bspBlock.BspReference.Tag;
                    var bspTag = bspItem.ReadMetadata<scenario_structure_bsp>();

                    ModuleItem stlmItem = null;
                    structure_lightmap stlmTag = null;
                    try
                    {
                        if (bspBlock.LightingVariants.Count > 0)
                        {
                            stlmItem = bspBlock.LightingVariants[0].StructureLightmapReference.Tag;
                            stlmTag = stlmItem.ReadMetadata<structure_lightmap>();
                        }
                    }
                    catch { stlmItem = null; stlmTag = null; }

                    // adjust where to get mesh resource data from
                    Halo5GeometryArgs geoParams;
                    if (stlmTag == null)
                    {
                        geoParams = new Halo5GeometryArgs
                        {
                            Module = bspItem.Module,
                            ResourcePolicy = ResourcePackingPolicy.SingleResource,
                            Materials = bspTag.Materials,
                            Sections = bspTag.Sections,
                            ResourceIndex = bspItem.ResourceIndex,
                            ResourceCount = bspItem.ResourceCount
                        };
                    }
                    else
                    {
                        geoParams = new Halo5GeometryArgs
                        {
                            Module = stlmItem.Module,
                            ResourcePolicy = ResourcePackingPolicy.SingleResource,
                            Materials = stlmTag.Materials,
                            Sections = stlmTag.Sections,
                            ResourceIndex = stlmItem.ResourceIndex + 1, // second resource is the geometry resource
                            ResourceCount = stlmItem.ResourceCount - 1
                        };
                    }

                    // copied straight from the h5 scenario_structure_bsp.cs
                    var model = new Model { Name = bspItem.FileName, OriginalPath = bspItem.TagName };

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
                                Transform = i.Transform,
                                Scale = (Vector3)i.TransformScale,
                                MeshRange = (i.MeshIndex, 1),
                                IsInstanced = true
                            })
                        );
                        model.Regions.Add(sectionRegion);
                    }

                    model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out var materials));
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

            if (bspGroup.HasItems)
                scene.ChildGroups.Add(bspGroup);

            #endregion

            #region Skies

            var skyGroup = new SceneGroup { Name = BlamConstants.ScenarioSkyGroupName };

            foreach (var skyTag in ReadTags<scenery>(Skies.EmptyIfNull().Select(b => b.SkyReference)))
            {
                try
                {
                    var provider = skyTag.ReadRenderModel() as IContentProvider<Model>;
                    skyGroup.ChildObjects.Add(provider.GetContent());
                }
                catch { }
            }

            if (skyGroup.HasItems)
                scene.ChildGroups.Add(skyGroup);

            #endregion

            ConfigurePlacements<scenery, SceneryPlacementBlock>(scene, BlamConstants.ScenarioSceneryGroupName, SceneryPalette, Scenery);
            ConfigurePlacements<device_machine, MachinePlacementBlock>(scene, BlamConstants.ScenarioMachineGroupName, MachinePalette, Machines);
            ConfigurePlacements<device_control, ControlPlacementBlock>(scene, BlamConstants.ScenarioControlGroupName, ControlPalette, Controls);
            ConfigurePlacements<crate, CratePlacementBlock>(scene, BlamConstants.ScenarioCrateGroupName, CratePalette, Crates);

            return scene;
        }

        private static IEnumerable<T> ReadTags<T>(IEnumerable<TagReference> collection)
        {
            return collection.Where(t => t.Tag != null)
                .DistinctBy(t => t.TagId)
                .Select(t => t.Tag.ReadMetadata<T>());
        }

        private static void ConfigurePlacements<TMetadata, TPlacementBlock>(Scene scene, string groupName, IEnumerable<ObjectPaletteBlock> palette, IEnumerable<TPlacementBlock> placements)
            where TMetadata : ObjectTagBase
            where TPlacementBlock : PlacementBlockBase
        {
            if (palette == null || placements == null)
                return;

            var parentGroup = new SceneGroup { Name = groupName };

            foreach (var group in placements.GroupBy(x => x.PaletteIndex))
            {
                var tagRef = palette.ElementAtOrDefault(group.Key)?.TagReference;
                if (tagRef?.Tag == null)
                    continue;

                Model model = null;
                try
                {
                    var tag = tagRef.Value.Tag.ReadMetadata<TMetadata>();
                    model = (tag.ReadRenderModel() as IContentProvider<Model>)?.GetContent();
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
                        sceneItem.SetTransform(placement.Scale, placement.Position.ToVector3(), (Quaternion)placement.Rotation);
                        placementGroup.ChildObjects.Add(sceneItem);
                    }
                    catch { }
                }

                if (placementGroup.HasItems)
                    parentGroup.ChildGroups.Add(placementGroup);
            }

            if (parentGroup.HasItems)
                scene.ChildGroups.Add(parentGroup);
        }
    }

    public partial class StructureBspBlock
    {
        public TagReference BspReference { get; set; }
        public BlockCollection<StructureLightingBlock> LightingVariants { get; set; }
    }

    public partial class StructureLightingBlock
    {
        public TagReference StructureLightmapReference { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(SkyReference)},nq}}")]
    public partial class SkyReferenceBlock
    {
        public TagReference SkyReference { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public partial class ObjectNameBlock
    {
        public StringId Name { get; set; }
        public short ObjectType { get; set; }
        public short PlacementIndex { get; set; }

        private string GetDebuggerDisplay() => $"[{ObjectType:X2}] {Name.Value}";
    }

    public abstract partial class PlacementBlockBase
    {
        public short PaletteIndex { get; set; }
        public short NameIndex { get; set; }
        public RealVector3 Position { get; set; }
        public EulerAngles Rotation { get; set; }
        public float Scale { get; set; }
    }

    public abstract partial class ObjectPlacementBlockBase : PlacementBlockBase
    {
        public StringId VariantName { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(TagReference)},nq}}")]
    public partial class ObjectPaletteBlock
    {
        public TagReference TagReference { get; set; }
    }

    public partial class SceneryPlacementBlock : ObjectPlacementBlockBase
    {

    }

    public partial class MachinePlacementBlock : ObjectPlacementBlockBase
    {

    }

    public partial class ControlPlacementBlock : ObjectPlacementBlockBase
    {

    }

    public partial class CratePlacementBlock : ObjectPlacementBlockBase
    {

    }
}
