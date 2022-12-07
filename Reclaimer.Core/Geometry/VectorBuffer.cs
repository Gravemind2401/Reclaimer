using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reclaimer.Geometry
{
    public class VectorBuffer<TVector> : BufferedCollection<TVector>, IVectorBuffer
        where TVector : struct, IBufferableVector<TVector>
    {
        private static readonly int TDimensions = (int)typeof(TVector).GetProperty(nameof(IVector.Dimensions), System.Reflection.BindingFlags.Static).GetValue(null);
        private static readonly bool TReadOnly = (bool)typeof(TVector).GetProperty(nameof(IVector.IsReadOnly), System.Reflection.BindingFlags.Static).GetValue(null);

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

        public void SwapEndianness()
        {
            if (TPack == 1)
                return;

            for (var i = 0; i < Count; i++)
            {
                var span = CreateSpan(i);
                for (var j = 0; j < TSize; j += TPack)
                    span.Slice(j, TPack).Reverse();
            }
        }

        int IVectorBuffer.Dimensions => TDimensions;
        IVectorBuffer IVectorBuffer.Slice(int index, int count) => Slice(index, count);
        IVector IReadOnlyList<IVector>.this[int index] => this[index];
        IEnumerator<IVector> IEnumerable<IVector>.GetEnumerator() => Enumerate().OfType<IVector>().GetEnumerator();
    }
}
