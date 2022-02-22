using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.HaloReach
{
    public class render_method_template
    {
        [Offset(72)]
        public BlockCollection<StringId> Arguments { get; set; }

        [Offset(108)]
        public BlockCollection<StringId> Usages { get; set; }
    }
}
