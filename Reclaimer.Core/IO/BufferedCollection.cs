namespace Reclaimer.IO
{
    public class BufferedCollection<TBufferable> : DataBuffer<TBufferable>, ICollection<TBufferable>
        where TBufferable : struct, IBufferable<TBufferable>
    {
        private static int TPack => TBufferable.PackSize;
        private static int TSize => TBufferable.SizeOf;

        protected override int SizeOf => TSize;

        /// <summary>
        /// Creates a new empty <see cref="BufferedCollection{TBufferable}"/> using a buffer sized to fit <paramref name="count"/> elements of <typeparamref name="TBufferable"/>.
        /// </summary>
        /// <inheritdoc cref="BufferedCollection{TBufferable}(byte[], int, int, int, int)"/>
        public BufferedCollection(int count)
            : base(new byte[count * TSize], count, 0, TSize, 0)
        { }

        /// <remarks>
        /// <list type="bullet">
        /// <item>The <c>count</c> value will inferred based on the size of <typeparamref name="TBufferable"/> and the length of the <paramref name="buffer"/>.</item>
        /// <item>The <c>start</c> value will default to 0.</item>
        /// <item>The <c>stride</c> value will default to the size of <typeparamref name="TBufferable"/>.</item>
        /// <item>The <c>offset</c> value will default to 0.</item>
        /// </list>
        /// </remarks>
        /// <inheritdoc cref="BufferedCollection{TBufferable}(byte[], int, int, int, int)"/>
        public BufferedCollection(byte[] buffer)
            : base(buffer, buffer?.Length / TSize ?? default, 0, TSize, 0)
        { }

        /// <remarks>
        /// <list type="bullet">
        /// <item>The <c>start</c> value will default to 0.</item>
        /// <item>The <c>stride</c> value will default to the size of <typeparamref name="TBufferable"/>.</item>
        /// <item>The <c>offset</c> value will default to 0.</item>
        /// </list>
        /// </remarks>
        /// <inheritdoc cref="BufferedCollection{TBufferable}(byte[], int, int, int, int)"/>
        public BufferedCollection(byte[] buffer, int count)
            : base(buffer, count, 0, TSize, 0)
        { }

        /// <remarks>
        /// <list type="bullet">
        /// <item>The <c>start</c> value will default to 0.</item>
        /// <item>The <c>offset</c> value will default to 0.</item>
        /// </list>
        /// </remarks>
        /// <inheritdoc cref="BufferedCollection{TBufferable}(byte[], int, int, int, int)"/>
        public BufferedCollection(byte[] buffer, int count, int stride)
            : base(buffer, count, 0, stride, 0)
        { }

        /// <remarks>
        /// <list type="bullet">
        /// <item>The <c>start</c> value will default to 0.</item>
        /// </list>
        /// </remarks>
        /// <inheritdoc cref="BufferedCollection{TBufferable}(byte[], int, int, int, int)"/>
        public BufferedCollection(byte[] buffer, int count, int stride, int offset)
            : base(buffer, count, 0, stride, offset)
        { }

        /// <summary>
        /// Creates a new <see cref="BufferedCollection{TBufferable}"/> from a byte array.
        /// </summary>
        /// <inheritdoc/>
        public BufferedCollection(byte[] buffer, int count, int start, int stride, int offset)
            : base(buffer, count, start, stride, offset)
        { }

        /// <inheritdoc/>
        public override TBufferable this[int index]
        {
            get => TBufferable.ReadFromBuffer(CreateSpan(index));
            set => value.WriteToBuffer(CreateSpan(index));
        }

        /// <summary>
        /// Reverses each block of bytes in the collection, where the block size is based on the value returned by <see cref="TBufferable.SizeOf"/>.
        /// </summary>
        public void ReverseEndianness()
        {
            if (TPack == 1)
                return;

            for (var i = 0; i < Count; i++)
                Utils.ReverseEndianness(CreateSpan(i), TPack);
        }

        /// <summary>
        /// Returns the array of unsigned bytes from which this collection was created.
        /// </summary>
        /// <returns>
        /// The byte array from which this collection was created, or the underlying array if
        /// a byte array was not provided to the <seealso cref="BufferedCollection{T}"/> constructor
        /// during construction of the current instance.
        /// </returns>
        public byte[] GetBuffer() => buffer;

        #region ICollection
        public void CopyTo(TBufferable[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            while (arrayIndex < array.Length && arrayIndex < Count)
                array[arrayIndex] = this[arrayIndex++];
        }

        bool ICollection<TBufferable>.IsReadOnly => true;
        void ICollection<TBufferable>.Add(TBufferable item) => throw new NotSupportedException();
        void ICollection<TBufferable>.Clear() => throw new NotSupportedException();
        bool ICollection<TBufferable>.Contains(TBufferable item) => throw new NotSupportedException();
        bool ICollection<TBufferable>.Remove(TBufferable item) => throw new NotSupportedException();
        #endregion
    }
}
