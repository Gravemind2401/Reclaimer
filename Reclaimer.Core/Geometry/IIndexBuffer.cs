using Reclaimer.IO;
using System.Diagnostics.CodeAnalysis;

namespace Reclaimer.Geometry
{
    public interface IIndexBuffer : IReadOnlyList<int>
    {
        internal static readonly IEqualityComparer<IIndexBuffer> EqualityComparer = new CustomEqualityComparer();
        
        IndexFormat Layout { get; }

        IIndexBuffer Slice(int index, int count);
        void ReverseEndianness();

        sealed int this[Index index] => ((IReadOnlyList<int>)this)[index.GetOffset(Count)];
        sealed IEnumerable<int> this[Range range] => Extensions.GetRange(this, range);

        private sealed class CustomEqualityComparer : IEqualityComparer<IIndexBuffer>
        {
            public bool Equals(IIndexBuffer x, IIndexBuffer y)
            {
                //ReferenceEquals() || underlying type equals || IDataBuffer equals
                return ReferenceEquals(x, y) || (x.GetHashCode() == y.GetHashCode() && x.Equals(y))
                    || (x is IDataBuffer bx && y is IDataBuffer by && IDataBuffer.Equals(bx, by));
            }

            public int GetHashCode([DisallowNull] IIndexBuffer obj) => HashCode.Combine(obj.Count, obj.Layout);
        }
    }
}
