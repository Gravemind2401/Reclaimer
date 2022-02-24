using Reclaimer.Blam.Common;
using System;
using System.Collections.Generic;
using Reclaimer.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo3
{
    public class scenario_lightmap
    {
        [Offset(4)]
        [MaxVersion((int)CacheType.MccHalo3U4)]
        public BlockCollection<scenario_lightmap_bsp_data> LightmapData { get; set; }

        [Offset(4)]
        [MinVersion((int)CacheType.MccHalo3U4)]
        public BlockCollection<TagReference> LightmapRefs { get; set; }
    }
}
