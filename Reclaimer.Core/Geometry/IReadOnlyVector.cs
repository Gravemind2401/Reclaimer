using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public interface IReadOnlyVector2
    {
        float X { get; }
        float Y { get; }
    }

    public interface IReadOnlyVector3 : IReadOnlyVector2
    {
        float Z { get; }
    }

    public interface IReadOnlyVector4 : IReadOnlyVector3
    {
        float W { get; }
    }
}
