using System;
using System.Collections.Generic;

namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        int Dimensions { get; }
        IVectorBuffer Slice(int index, int count);
        void SwapEndianness();
        IVector this[Index index] => ((IReadOnlyList<IVector>)this)[index.GetOffset(Count)];
        IEnumerable<IVector> this[Range range] => Extensions.GetRange(this, range);
    }
}
