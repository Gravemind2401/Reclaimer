using System.Diagnostics.CodeAnalysis;

namespace Reclaimer.IO
{
    internal interface IDataBuffer
    {
        public static readonly IEqualityComparer<IDataBuffer> EqualityComparer = new CustomEqualityComparer();

        Type DataType { get; }
        int SizeOf { get; }
        int Count { get; }
        ReadOnlySpan<byte> Buffer { get; }
        int Start { get; }
        int Stride { get; }
        int Offset { get; }
        ReadOnlySpan<byte> GetBytes(int index);

        public static sealed bool Equals(IDataBuffer x, IDataBuffer y) => EqualityComparer.GetHashCode(x) == EqualityComparer.GetHashCode(y) && EqualityComparer.Equals(x, y);

        private sealed class CustomEqualityComparer : IEqualityComparer<IDataBuffer>
        {
            public bool Equals(IDataBuffer x, IDataBuffer y) => ReferenceEquals(x, y) || x.Buffer == y.Buffer || x.Buffer.SequenceEqual(y.Buffer);
            public int GetHashCode([DisallowNull] IDataBuffer obj) => HashCode.Combine(obj.DataType, obj.Count, obj.SizeOf, obj.Start, obj.Stride, obj.Offset);
        }
    }
}
