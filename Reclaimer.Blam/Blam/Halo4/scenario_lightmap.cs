using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class scenario_lightmap
    {
        [Offset(4)]
        public BlockCollection<LightmapDataInfoBlock> LightmapRefs { get; set; }
    }

    [FixedSize(32)]
    public class LightmapDataInfoBlock
    {
        [Offset(0)]
        public TagReference LightmapDataReference { get; set; }
    }
}
