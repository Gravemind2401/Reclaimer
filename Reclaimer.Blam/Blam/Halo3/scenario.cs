using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Geometry;

namespace Reclaimer.Blam.Halo3
{
    public partial class scenario : ContentTagDefinition<Scene>
    {
        public scenario(IIndexItem item)
            : base(item)
        { }

        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public BlockCollection<SkyReferenceBlock> Skies { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }

        public override Scene GetContent()
        {
            var scene = new Scene { Name = Item.FileName, CoordinateSystem = CoordinateSystem2.Default.WithScale(BlamConstants.Gen3UnitScale) };
            var bspGroup = new SceneGroup { Name = BlamConstants.ScenarioBspGroupName };
            var skyGroup = new SceneGroup { Name = BlamConstants.ScenarioSkyGroupName };

            //TODO: display error models in some way

            foreach (var bspTag in ReadTags<scenario_structure_bsp>(StructureBsps.Select(b => b.BspReference)))
            {
                try
                {
                    var provider = bspTag as IContentProvider<Model>;
                    bspGroup.ChildObjects.Add(provider.GetContent());
                }
                catch { }
            }

            foreach (var skyTag in ReadTags<scenery>(Skies.Select(b => b.SkyReference)))
            {
                try
                {
                    var provider = skyTag.ReadRenderModel() as IContentProvider<Model>;
                    var model = provider.GetContent();
                    model.Flags |= SceneFlags.SkyFlag;
                    skyGroup.ChildObjects.Add(model);
                }
                catch { }
            }

            if (bspGroup.ChildObjects.Count > 0)
                scene.ChildGroups.Add(bspGroup);

            if (skyGroup.ChildObjects.Count > 0)
                scene.ChildGroups.Add(skyGroup);

            return scene;
        }

        private static IEnumerable<T> ReadTags<T>(IEnumerable<TagReference> collection) => collection.Where(t => t.IsValid).DistinctBy(t => t.TagId).Select(t => t.Tag.ReadMetadata<T>());
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
}
