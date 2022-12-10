using Reclaimer.IO;
using System.Collections.Generic;
using System.Linq;

namespace Reclaimer.Geometry
{
    public class VectorBuffer<TVector> : BufferedCollection<TVector>, IVectorBuffer
        where TVector : struct, IBufferableVector<TVector>
    {
        public VectorBuffer(int count)
            : base(count)
        { }

        public VectorBuffer(byte[] buffer)
            : base(buffer)
        { }

        public VectorBuffer(byte[] buffer, int count)
            : base(buffer, count)
        { }

        public VectorBuffer(byte[] buffer, int count, int stride)
            : base(buffer, count, stride)
        { }

        public VectorBuffer(byte[] buffer, int count, int stride, int offset)
            : base(buffer, count, stride, offset)
        { }

        public VectorBuffer(byte[] buffer, int count, int start, int stride, int offset)
            : base(buffer, count, start, stride, offset)
        { }

        public VectorBuffer<TVector> Slice(int index, int count)
        {
            var newStart = start + index * stride;
            return new VectorBuffer<TVector>(buffer, count, newStart, stride, offset);
        }

        int IVectorBuffer.Dimensions => TVector.Dimensions;
        IVectorBuffer IVectorBuffer.Slice(int index, int count) => Slice(index, count);
        IVector IReadOnlyList<IVector>.this[int index] => this[index];
        IEnumerator<IVector> IEnumerable<IVector>.GetEnumerator() => Enumerate().OfType<IVector>().GetEnumerator();
    }
}
