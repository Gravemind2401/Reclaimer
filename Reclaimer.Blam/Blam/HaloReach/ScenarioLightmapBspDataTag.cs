using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloReach
{
    public class ScenarioLightmapBspDataTag
    {
        [Offset(112, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(124, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(244, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(268, MinVersion = (int)CacheType.HaloReachRetail)]
        public ResourceIdentifier ResourcePointer { get; set; }
    }
}
