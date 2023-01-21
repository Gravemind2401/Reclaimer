using Reclaimer.Blam.Common;
using Reclaimer.Geometry;

namespace Reclaimer.Blam.Halo3
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
            var scene = new Scene { Name = Item.FileName };
            var group = new SceneGroup { Name = nameof(scenario_structure_bsp) };

            scene.ObjectGroups.Add(group);

            foreach (var bspTag in StructureBsps.Select(b => b.BspReference.Tag))
            {
                var bspData = bspTag.ReadMetadata<scenario_structure_bsp>();
                var sceneObj = new SceneObject
                {
                    Name = bspTag.FileName,
                    Model = bspData.GetContent()
                };

                group.ChildObjects.Add(sceneObj);
            }

            return scene;
        }
    }

    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public partial class StructureBspBlock
    {
        public TagReference BspReference { get; set; }
    }
}
