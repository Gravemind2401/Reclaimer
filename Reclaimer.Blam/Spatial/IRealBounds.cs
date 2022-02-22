using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public interface IRealBounds
    {
        float Min { get; set; }
        float Max { get; set; }
        float Length { get; }
        float Midpoint { get; }
    }
}
