using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
