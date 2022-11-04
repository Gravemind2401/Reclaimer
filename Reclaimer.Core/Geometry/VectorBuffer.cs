using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        int IVectorBuffer.Dimensions => TDimensions;
        IVector IReadOnlyList<IVector>.this[int index] => this[index];
        IEnumerator<IVector> IEnumerable<IVector>.GetEnumerator() => Enumerate().OfType<IVector>().GetEnumerator();
    }
}
