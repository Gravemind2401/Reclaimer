using System;
using System.Collections;
using System.Collections.Generic;

namespace Reclaimer.Geometry
{
    public interface IIndexBuffer : IReadOnlyList<int>
    {
        IndexFormat Layout { get; }

        IIndexBuffer Slice(int index, int count);
        void ReverseEndianness();

        sealed int this[Index index] => this[index.GetOffset(Count)];
        sealed IEnumerable<int> this[Range range] => Extensions.GetRange(this, range);
    }
}
