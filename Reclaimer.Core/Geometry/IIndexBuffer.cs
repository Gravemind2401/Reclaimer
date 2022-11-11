using System;
using System.Collections.Generic;

namespace Reclaimer.Geometry
{
    public interface IIndexBuffer : IReadOnlyList<int>
    {
        void SwapEndianness();
        int this[Index index] => this[index.GetOffset(Count)];
        IEnumerable<int> this[Range range] => Extensions.Subset(this, range);
    }
}
