using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    //in Halo3Beta and Halo3Retail this is embedded within the scenario_lightmap tag.
    //in Halo3ODST it was separated into its own tag type.

    [FixedSize(436, MaxVersion = (int)CacheType.Halo3ODST)]
    public class scenario_lightmap_bsp_data
    {
        [Offset(2)]
        public short BspIndex { get; set; }

        [Offset(308)]
        public BlockCollection<SectionBlock> Sections { get; set; }

        [Offset(428)]
        public ResourceIdentifier ResourcePointer { get; set; }
    }
}
