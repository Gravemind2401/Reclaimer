using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.HaloReach
{
    public class scenario
    {
        [Offset(68, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(76, MinVersion = (int)CacheType.HaloReachRetail)]
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }

        [Offset(1828, MaxVersion = (int)CacheType.HaloReachRetail)]
        [Offset(1844, MinVersion = (int)CacheType.HaloReachRetail, MaxVersion = (int)CacheType.MccHaloReach)]
        [Offset(1856, MinVersion = (int)CacheType.MccHaloReach)]
        public TagReference ScenarioLightmapReference { get; set; }
    }

    [FixedSize(172)]
    public class StructureBspBlock
    {
        [Offset(0)]
        public TagReference BspReference { get; set; }
    }
}
