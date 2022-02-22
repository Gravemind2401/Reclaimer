using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
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
