using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class ScenarioLightmapTag
    {
        [Offset(4)]
        [MaxVersion((int)CacheType.MccHalo3U4)]
        public BlockCollection<ScenarioLightmapBspDataTag> LightmapData { get; set; }

        [Offset(4)]
        [MinVersion((int)CacheType.MccHalo3U4)]
        public BlockCollection<TagReference> LightmapRefs { get; set; }
    }
}
