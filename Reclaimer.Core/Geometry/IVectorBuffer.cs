namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        /// <summary>
        /// Gets the minimum number of axes available in each vector.
        /// </summary>
        int Dimensions { get; }

        /// <summary>
        /// Creates a new <see cref="VectorBuffer{TVector}"/> using the same underlying byte array, but only spanning the specified range of elements.
        /// </summary>
        /// <param name="index">The index from which to start the new buffer.</param>
        /// <param name="count">The number of elements to include in the new buffer.</param>
        IVectorBuffer Slice(int index, int count);

        /// <summary>
        /// Reverses each block of bytes in the collection, where the block size is the size of each buffer element.
        /// </summary>
        void ReverseEndianness();

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        sealed IVector this[Index index] => ((IReadOnlyList<IVector>)this)[index.GetOffset(Count)];

        /// <summary>
        /// Gets the subset of elements within the specified range.
        /// </summary>
        sealed IEnumerable<IVector> this[Range range] => Extensions.GetRange(this, range);
    }
}
