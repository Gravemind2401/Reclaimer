using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public interface IRealBounds5D
    {
        IRealBounds XBounds { get; }
        IRealBounds YBounds { get; }
        IRealBounds ZBounds { get; }
        IRealBounds UBounds { get; }
        IRealBounds VBounds { get; }
    }
}
