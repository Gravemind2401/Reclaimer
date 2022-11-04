using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        int Dimensions { get; }
    }
}
