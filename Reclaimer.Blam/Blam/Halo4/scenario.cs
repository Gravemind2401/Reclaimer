using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Numerics;

namespace Reclaimer.Blam.Halo4
{
    public partial class scenario : ContentTagDefinition<Scene>
    {
        public scenario(IIndexItem item)
            : base(item)
        { }

        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public BlockCollection<SkyReferenceBlock> Skies { get; set; }
        public BlockCollection<ObjectNameBlock> ObjectNames { get; set; }
        public BlockCollection<SceneryPlacementBlock> Scenery { get; set; }
        public BlockCollection<ObjectPaletteBlock> SceneryPalette { get; set; }
        public BlockCollection<MachinePlacementBlock> Machines { get; set; }
        public BlockCollection<ObjectPaletteBlock> MachinePalette { get; set; }
        public BlockCollection<ControlPlacementBlock> Controls { get; set; }
        public BlockCollection<ObjectPaletteBlock> ControlPalette { get; set; }
        public BlockCollection<CratePlacementBlock> Crates { get; set; }
        public BlockCollection<ObjectPaletteBlock> CratePalette { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }

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

            foreach (var bspTag in ReadTags<scenario_structure_bsp>(StructureBsps.Select(b => b.BspReference)))
            {
                try
                {
                    var provider = bspTag as IContentProvider<Model>;
                    var model = provider.GetContent();
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
            return collection.Where(t => t.IsValid)
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
                if (!tagRef.HasValue || !tagRef.Value.IsValid)
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

    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public partial class StructureBspBlock
    {
        [Offset(0)]
        public TagReference BspReference { get; set; }
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
