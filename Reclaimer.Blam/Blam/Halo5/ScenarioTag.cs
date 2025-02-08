using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.Utilities;
using System.Globalization;
using System.Numerics;

namespace Reclaimer.Blam.Halo5
{
    public partial class ScenarioTag : ContentTagDefinition<Scene>, IContentProvider<Scene>
    {
        public ScenarioTag(ModuleItem item, MetadataHeader header)
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
                    var bspItem = (ModuleItem)bspBlock.BspReference.Tag;
                    var bspTag = bspItem.ReadMetadata<ScenarioStructureBspTag>();

                    ModuleItem stlmItem = null;
                    StructureLightmapTag stlmTag = null;
                    try
                    {
                        if (bspBlock.LightingVariants.Count > 0)
                        {
                            stlmItem = (ModuleItem)bspBlock.LightingVariants[0].StructureLightmapReference.Tag;
                            stlmTag = stlmItem.ReadMetadata<StructureLightmapTag>();
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

                    var visibleInstances = bspTag.GeometryInstances.Where(i => !i.FlagsOverride.HasFlag(MeshFlags.MeshIsCustomShadowCaster));

                    foreach (var instanceGroup in BlamUtils.GroupGeometryInstances(visibleInstances, i => i.Name.Value))
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

                    model.Meshes.AddRange(Halo5Common.GetMeshes(geoParams, out _));

                    foreach (var instanceGroup in visibleInstances)
                    {
                        var mesh = model.Meshes[instanceGroup.MeshIndex];
                        if (mesh == null)
                            continue;

                        var bounds = bspTag.BoundingBoxes[instanceGroup.BoundsIndex];
                        var posBounds = new RealBounds3D(bounds.XBounds, bounds.YBounds, bounds.ZBounds);
                        var texBounds = new RealBounds2D(bounds.UBounds, bounds.VBounds);

                        (mesh.PositionBounds, mesh.TextureBounds) = (posBounds, texBounds);
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

            foreach (var skyTag in ReadTags<SceneryTag>(Skies.EmptyIfNull().Select(b => b.SkyReference)))
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

            ConfigurePlacements<SceneryTag, SceneryPlacementBlock>(scene, BlamConstants.ScenarioSceneryGroupName, SceneryPalette, Scenery);
            ConfigurePlacements<DeviceMachineTag, MachinePlacementBlock>(scene, BlamConstants.ScenarioMachineGroupName, MachinePalette, Machines);
            ConfigurePlacements<DeviceControlTag, ControlPlacementBlock>(scene, BlamConstants.ScenarioControlGroupName, ControlPalette, Controls);
            ConfigurePlacements<CrateTag, CratePlacementBlock>(scene, BlamConstants.ScenarioCrateGroupName, CratePalette, Crates);

            return scene;
        }

        private static IEnumerable<T> ReadTags<T>(IEnumerable<TagReferenceGen5> collection)
        {
            return collection.Where(t => t.Tag != null)
                .DistinctBy(t => t.TagId)
                .Select(t => t.Tag.ReadMetadata<T>());
        }

        private static void ConfigurePlacements<TMetadata, TPlacementBlock>(Scene scene, string groupName, IEnumerable<ObjectPaletteBlock> palette, IEnumerable<TPlacementBlock> placements)
            where TMetadata : ObjectTag
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
        public TagReferenceGen5 BspReference { get; set; }
        public BlockCollection<StructureLightingBlock> LightingVariants { get; set; }
    }

    public partial class StructureLightingBlock
    {
        public TagReferenceGen5 StructureLightmapReference { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(SkyReference)},nq}}")]
    public partial class SkyReferenceBlock
    {
        public TagReferenceGen5 SkyReference { get; set; }
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
        public TagReferenceGen5 TagReference { get; set; }
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
