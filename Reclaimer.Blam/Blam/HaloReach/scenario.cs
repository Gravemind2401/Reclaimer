using Reclaimer.Blam.Common;
using Reclaimer.Geometry;
using Reclaimer.Utilities;
using System.Text.RegularExpressions;

namespace Reclaimer.Blam.HaloReach
{
    public partial class scenario : ContentTagDefinition<Scene>
    {
        public scenario(IIndexItem item)
            : base(item)
        { }

        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }

        public override Scene GetContent()
        {
            var scene = new Scene { Name = Item.FileName, CoordinateSystem = CoordinateSystem.Default.WithScale(BlamConstants.Gen3UnitScale) };
            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };

            //TODO: display error models in some way

            foreach (var bspTag in ReadTags<scenario_structure_bsp>(StructureBsps.Select(b => b.BspReference)))
            {
                try
                {
                    var provider = bspTag as IContentProvider<Model>;
                    var model = provider.GetContent();
                    if (!Regex.IsMatch(model.Name, "_(?:000|shared)$"))
                        model.Flags |= SceneFlags.PrimaryFocus;
                    bspGroup.ChildObjects.Add(model);
                }
                catch { }
            }

            if (bspGroup.HasItems)
                scene.ChildGroups.Add(bspGroup);

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
}
