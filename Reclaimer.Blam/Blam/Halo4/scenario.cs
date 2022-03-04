using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo4
{
    public class scenario
    {
        [Offset(152, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(160, MinVersion = (int)CacheType.Halo4Retail)]
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }

        [Offset(1884, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(1896, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4)]
        [Offset(1908, MinVersion = (int)CacheType.MccHalo4)]
        public TagReference ScenarioLightmapReference { get; set; }
    }

    [FixedSize(296, MaxVersion = (int)CacheType.Halo4Retail)]
    [FixedSize(336, MinVersion = (int)CacheType.Halo4Retail)]
    public class StructureBspBlock
    {
        [Offset(0)]
        public TagReference BspReference { get; set; }
    }
}
