using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class scenario_lightmap
    {
        [Offset(4)]
        [MaxVersion((int)CacheType.Halo3ODST)]
        public BlockCollection<scenario_lightmap_bsp_data> LightmapData { get; set; }

        [Offset(4)]
        [MinVersion((int)CacheType.Halo3ODST)]
        public BlockCollection<TagReference> LightmapRefs { get; set; }
    }
}
