using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.MccHalo4
{
    public class PointerExpander : IPointerExpander
    {
        private readonly int magic;

        public PointerExpander(CacheFile cache)
        {
            magic = 0x4FFF0000;
        }

        public long Expand(int pointer) => ((long)pointer << 2) + magic;
        public int Contract(long pointer) => (int)((pointer - magic) >> 2);
    }
}
