using System.Collections;

namespace Reclaimer.Geometry
{
    public partial class IndexBuffer
    {
        public static IIndexBuffer Transform(IIndexBuffer buffer, int offset) => new TransformedIndexBuffer(buffer, offset);

        private sealed class TransformedIndexBuffer : IIndexBuffer
        {
            private readonly IIndexBuffer source;
            private readonly int offset;

            public TransformedIndexBuffer(IIndexBuffer source, int offset)
            {
                if (source is TransformedIndexBuffer t)
                {
                    this.source = t.source;
                    this.offset = t.offset + offset;
                }
                else
                {
                    this.source = source;
                    this.offset = offset;
                }
            }

            public int this[int index] => source[index] + offset;
            public int Count => source.Count;

            IndexFormat IIndexBuffer.Layout => source.Layout;

            IIndexBuffer IIndexBuffer.Slice(int index, int count) => new TransformedIndexBuffer(source.Slice(index, count), offset);
            void IIndexBuffer.ReverseEndianness() => throw new NotSupportedException("Operation not supported on a transformed index buffer");

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
