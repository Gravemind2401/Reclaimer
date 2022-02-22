using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public interface IRealVector3D
    {
        float X { get; set; }
        float Y { get; set; }
        float Z { get; set; }
        float Length { get; }
    }
}
