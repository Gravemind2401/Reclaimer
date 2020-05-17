using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IPointerExpander
    {
        long Expand(int pointer);
    }
}
