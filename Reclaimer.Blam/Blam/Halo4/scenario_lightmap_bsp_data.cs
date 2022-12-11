using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class scenario_lightmap_bsp_data
    {
        [Offset(532, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(320, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(556, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(344, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<BoundingBoxBlock> BoundingBoxes { get; set; }

        [Offset(664, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(464, MinVersion = (int)CacheType.Halo4Retail)]
        public ResourceIdentifier ResourcePointer { get; set; }
    }
}
