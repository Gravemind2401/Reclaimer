using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.MccHalo2X
{
    public class PointerExpander : IPointerExpander
    {
        private readonly int magic;

        public PointerExpander(CacheFile cache)
        {
            magic = 0x7AC00000;
        }

        public long Expand(int pointer)
        {
            return ((long)pointer << 2) + magic;
        }
    }
}
