using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public class LanguageDefinition
    {
        [Offset(0)]
        public int StringCount { get; set; }

        [Offset(4)]
        public int StringsSize { get; set; }

        [Offset(8)]
        public int IndicesOffset { get; set; }

        [Offset(12)]
        public int StringsOffset { get; set; }
    }
}
