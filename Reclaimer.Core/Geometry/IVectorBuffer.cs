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
        void SwapEndianness();
        IVector this[Index index] => this[index.GetOffset(Count)];
        IEnumerable<IVector> this[Range range] => Extensions.Subset(this, range);
    }
}
