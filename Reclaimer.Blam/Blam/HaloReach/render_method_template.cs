using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.HaloReach
{
    public class render_method_template
    {
        [Offset(72)]
        public BlockCollection<StringId> Arguments { get; set; }

        [Offset(108)]
        public BlockCollection<StringId> Usages { get; set; }
    }
}
