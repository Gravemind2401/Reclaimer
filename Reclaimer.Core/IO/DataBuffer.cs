using System.Collections;

namespace Reclaimer.IO
{
    public abstract class DataBuffer<T> : IDataBuffer, IReadOnlyList<T>
        where T : struct
    {
        protected readonly byte[] buffer;
        protected readonly int start;
        protected readonly int stride;
        protected readonly int offset;

        protected abstract int SizeOf { get; }

        /// <summary>
        /// The number of elements in the buffer.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataBuffer{T}"/> from a byte array.
        /// </summary>
        /// <param name="buffer">
        /// The byte array representing the buffer to read data from.
        /// </param>
        /// <param name="count">
        /// The number of elements in the buffer.
        /// </param>
        /// <param name="start">
        /// The offset within the byte array to start reading data from. This is where the <paramref name="stride"/> begins from.
        /// </param>
        /// <param name="stride">
        /// The number of bytes between the start of each element in the byte array.
        /// </param>
        /// <param name="offset">
        /// The offset of each element within the <paramref name="stride"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        protected DataBuffer(byte[] buffer, int count, int start, int stride, int offset)
        {
            this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (start < 0 || (start > 0 && start >= buffer.Length))
                throw new ArgumentOutOfRangeException(nameof(start));

            if (stride < 0 || (buffer.Length > 0 && stride > buffer.Length))
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

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public abstract T this[int index] { get; set; }

        /// <inheritdoc cref="this[int]"/>
        public T this[Index index]
        {
            get => this[index.GetOffset(Count)];
            set => this[index.GetOffset(Count)] = value;
        }

        /// <summary>
        /// Gets the subset of elements within the specified range.
        /// </summary>
        public IEnumerable<T> this[Range range] => Subset(range);

        /// <inheritdoc cref="this[Range]"/>
        public IEnumerable<T> Subset(Range range) => Extensions.GetRange(this, range);
        
        /// <param name="index">The index to begin returning elements from.</param>
        /// <param name="length">The the number of elements to return.</param>
        /// <inheritdoc cref="this[Range]"/>
        public IEnumerable<T> Subset(int index, int length) => Extensions.GetSubset(this, index, length);

        #region IDataBuffer
        Type IDataBuffer.DataType => typeof(T);
        int IDataBuffer.Count => Count;
        int IDataBuffer.SizeOf => SizeOf;
        ReadOnlySpan<byte> IDataBuffer.Buffer => buffer;
        int IDataBuffer.Start => start;
        int IDataBuffer.Stride => stride;
        int IDataBuffer.Offset => offset;
        ReadOnlySpan<byte> IDataBuffer.GetBytes(int index) => CreateSpan(index);
        #endregion

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
