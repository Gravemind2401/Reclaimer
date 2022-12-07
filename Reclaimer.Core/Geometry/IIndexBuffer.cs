using System;
using System.Collections.Generic;

namespace Reclaimer.Geometry
{
    public interface IIndexBuffer : IReadOnlyList<int>
    {
        IIndexBuffer Slice(int index, int count);
        void SwapEndianness();
        int this[Index index] => this[index.GetOffset(Count)];
        IEnumerable<int> this[Range range] => Extensions.GetRange(this, range);
    }
}
