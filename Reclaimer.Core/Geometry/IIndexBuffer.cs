using System;
using System.Collections;
using System.Collections.Generic;

namespace Reclaimer.Geometry
{
    public interface IIndexBuffer : IReadOnlyList<int>
    {
        IIndexBuffer Slice(int index, int count);
        void SwapEndianness();

        sealed int this[Index index] => this[index.GetOffset(Count)];
        sealed IEnumerable<int> this[Range range] => Extensions.GetRange(this, range);

        sealed IIndexBuffer AsTransformed(int offset) => new TransformedIndexBuffer(this, offset);

        private sealed class TransformedIndexBuffer : IIndexBuffer
        {
            private readonly IIndexBuffer source;
            private readonly int offset;

            public TransformedIndexBuffer(IIndexBuffer source, int offset)
            {
                this.source = source;
                this.offset = offset;
            }

            public int this[int index] => source[index] + offset;
            public int Count => source.Count;

            IIndexBuffer IIndexBuffer.Slice(int index, int count) => new TransformedIndexBuffer(source.Slice(index, count), offset);
            void IIndexBuffer.SwapEndianness() => throw new NotSupportedException("Operation not supported on a transformed buffer");

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerator.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Enumerator).GetEnumerator();

            private IEnumerable<int> Enumerator
            {
                get
                {
                    for (var i = 0; i < source.Count; i++)
                        yield return this[i];
                }
            }
        }
    }
}
