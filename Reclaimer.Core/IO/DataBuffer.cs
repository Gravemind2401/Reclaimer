using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public abstract class DataBuffer<T> : IReadOnlyList<T>
        where T : struct
    {
        protected readonly byte[] buffer;
        private readonly int start;
        private readonly int stride;
        private readonly int offset;

        protected abstract int SizeOf { get; }

        public int Count { get; }

        protected DataBuffer(byte[] buffer, int count, int start, int stride, int offset)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (start < 0 || start >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(start));

            if (stride < 0 || stride > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(stride));

            if (offset < 0 || offset + SizeOf > stride)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (buffer.Length < start + stride * count)
                throw new ArgumentException("Insufficient buffer length", nameof(buffer));

            Count = count;
            this.start = start;
            this.stride = stride;
            this.offset = offset;
        }

        protected Span<byte> CreateSpan(int index) => buffer.AsSpan(start + index * stride + offset, SizeOf);

        public abstract T this[int index] { get; set; }

        #region IEnumerable
        protected IEnumerable<T> Enumerate()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Enumerate()).GetEnumerator();
        #endregion
    }
}
