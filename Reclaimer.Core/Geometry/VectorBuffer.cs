using Reclaimer.IO;

namespace Reclaimer.Geometry
{
    public class VectorBuffer<TVector> : BufferedCollection<TVector>, IVectorBuffer
        where TVector : struct, IBufferableVector<TVector>
    {
        /// <summary>
        /// Creates a new empty <see cref="VectorBuffer{TVector}"/> using a buffer sized to fit <paramref name="count"/> elements.
        /// </summary>
        /// <inheritdoc/>
        public VectorBuffer(int count)
            : base(count)
        { }

        /// <inheritdoc cref="VectorBuffer{TVector}(byte[], int, int, int, int)"/>
        /// <inheritdoc/>
        public VectorBuffer(byte[] buffer)
            : base(buffer)
        { }

        /// <inheritdoc cref="VectorBuffer{TVector}(byte[], int, int, int, int)"/>
        /// <inheritdoc/>
        public VectorBuffer(byte[] buffer, int count)
            : base(buffer, count)
        { }

        /// <inheritdoc cref="VectorBuffer{TVector}(byte[], int, int, int, int)"/>
        /// <inheritdoc/>
        public VectorBuffer(byte[] buffer, int count, int stride)
            : base(buffer, count, stride)
        { }

        /// <inheritdoc cref="VectorBuffer{TVector}(byte[], int, int, int, int)"/>
        /// <inheritdoc/>
        public VectorBuffer(byte[] buffer, int count, int stride, int offset)
            : base(buffer, count, stride, offset)
        { }

        /// <summary>
        /// Creates a new <see cref="VectorBuffer{TVector}"/> from a byte array.
        /// </summary>
        /// <inheritdoc/>
        public VectorBuffer(byte[] buffer, int count, int start, int stride, int offset)
            : base(buffer, count, start, stride, offset)
        { }

        /// <summary>
        /// Creates a new <see cref="VectorBuffer{TVector}"/> using the same underlying byte array, but only spanning the specified range of elements.
        /// </summary>
        /// <param name="index">The index from which to start the new buffer.</param>
        /// <param name="count">The number of elements to include in the new buffer.</param>
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
