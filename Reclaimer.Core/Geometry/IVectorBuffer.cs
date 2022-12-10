using Reclaimer.Geometry.Vectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        int Dimensions { get; }
        IVectorBuffer Slice(int index, int count);
        void SwapEndianness();

        sealed IVector this[Index index] => ((IReadOnlyList<IVector>)this)[index.GetOffset(Count)];
        sealed IEnumerable<IVector> this[Range range] => Extensions.GetRange(this, range);
    }
}
