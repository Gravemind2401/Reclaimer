using Reclaimer.Blam.Common;

namespace Reclaimer.Blam.HaloReach
{
    public partial class scenario : ContentTagDefinition
    {
        public scenario(IIndexItem item)
            : base(item)
        { }

        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public partial class StructureBspBlock
    {
        public TagReference BspReference { get; set; }
    }
}
