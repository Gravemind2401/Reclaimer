using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class scenario
    {
        [Offset(68, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(76, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReachU13)]
        [Offset(80, MinVersion = (int)CacheType.MccHaloReachU13)]
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }

        [Offset(1828, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(1844, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(1856, MinVersion = (int)CacheType.MccHaloReach, MaxVersion = (int)CacheType.MccHaloReachU13)]
        [Offset(1800, MinVersion = (int)CacheType.MccHaloReachU13)]
        public TagReference ScenarioLightmapReference { get; set; }
    }

    [FixedSize(172)]
    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public class StructureBspBlock
    {
        [Offset(0)]
        public TagReference BspReference { get; set; }
    }
}
