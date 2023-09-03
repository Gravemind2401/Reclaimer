using Reclaimer.Blam.Common;

namespace Reclaimer.Blam.Halo3
{
    public partial class scenario
    {
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
        public TagReference ScenarioLightmapReference { get; set; }
    }

    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public partial class StructureBspBlock
    {
        public TagReference BspReference { get; set; }
    }
}
