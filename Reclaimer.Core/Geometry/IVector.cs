using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public interface IVector2
    {
        float X { get; set; }
        float Y { get; set; }
    }

    public interface IVector3 : IVector2
    {
        float Z { get; set; }
    }

    public interface IVector4 : IVector3
    {
        float W { get; set; }
    }
}
