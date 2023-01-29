namespace Reclaimer.Geometry
{
    public interface IVectorBuffer : IReadOnlyList<IVector>
    {
        int Dimensions { get; }
        IVectorBuffer Slice(int index, int count);
        void ReverseEndianness();

        sealed IVector this[Index index] => ((IReadOnlyList<IVector>)this)[index.GetOffset(Count)];
        sealed IEnumerable<IVector> this[Range range] => Extensions.GetRange(this, range);

        //These members are used for model exports to write compressed vectors.
        //Any unsupported types will just be exported as uncompressed float2/3/4.
        internal Type VectorType => null;
        internal int VectorSize => 0;
        internal ReadOnlySpan<byte> GetBytes(int index) => Span<byte>.Empty;
    }
}
