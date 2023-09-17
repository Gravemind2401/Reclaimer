using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;
using Reclaimer.Geometry.Vectors;
using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Blam.Halo3
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
        public BlockCollection<SceneryPaletteBlock> SceneryPalette { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }

        public override Scene GetContent()
        {
            var scene = new Scene { Name = Item.FileName, CoordinateSystem = CoordinateSystem2.Default.WithScale(BlamConstants.Gen3UnitScale) };
            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };
            var skyGroup = new SceneGroup { Name = BlamConstants.ScenarioSkyGroupName };
            var sceneryGroup = new SceneGroup { Name = BlamConstants.ScenarioSceneryGroupName };

            //TODO: display error models in some way

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

            foreach (var skyTag in ReadTags<scenery>(Skies.Select(b => b.SkyReference)))
            {
                try
                {
                    var provider = skyTag.ReadRenderModel() as IContentProvider<Model>;
                    skyGroup.ChildObjects.Add(provider.GetContent());
                }
                catch { }
            }

            foreach (var group in Scenery.EmptyIfNull().GroupBy(x => x.PaletteIndex))
            {
                var tagRef = SceneryPalette?.ElementAtOrDefault(group.Key)?.TagReference;
                if (!tagRef.HasValue || !tagRef.Value.IsValid)
                    continue;

                Model model = null;
                try
                {
                    var tag = tagRef.Value.Tag.ReadMetadata<scenery>();
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
                    sceneryGroup.ChildGroups.Add(placementGroup);
            }

            if (bspGroup.HasItems)
                scene.ChildGroups.Add(bspGroup);

            if (skyGroup.HasItems)
                scene.ChildGroups.Add(skyGroup);

            if (sceneryGroup.HasItems)
                scene.ChildGroups.Add(sceneryGroup);

            return scene;
        }

        private static IEnumerable<T> ReadTags<T>(IEnumerable<TagReference> collection)
        {
            return collection.Where(t => t.IsValid)
                .DistinctBy(t => t.TagId)
                .Select(t => t.Tag.ReadMetadata<T>());
        }
    }

    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public partial class StructureBspBlock
    {
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
        public string Name { get; set; }
        public short ObjectType { get; set; }
        public short PlacementIndex { get; set; }

        private string GetDebuggerDisplay() => $"[{ObjectType:X2}] {Name}";
    }

    public abstract partial class PlacementBlockBase
    {
        public short PaletteIndex { get; set; }
        public short NameIndex { get; set; }
        public RealVector3 Position { get; set; }
        public EulerAngles Rotation { get; set; }
        public float Scale { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(TagReference)},nq}}")]
    public abstract class PaletteBlockBase
    {
        public TagReference TagReference { get; set; }
    }

    public partial class SceneryPlacementBlock : PlacementBlockBase
    {
        public StringId VariantName { get; set; }
    }

    public partial class SceneryPaletteBlock : PaletteBlockBase
    {

    }
}
