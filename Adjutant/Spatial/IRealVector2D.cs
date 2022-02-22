using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public interface IRealVector2D
    {
        float X { get; set; }
        float Y { get; set; }
        float Length { get; }
    }
}
