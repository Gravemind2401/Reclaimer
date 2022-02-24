using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public interface IPointerExpander
    {
        long Expand(int pointer);
        int Contract(long pointer);
    }
}
