using Reclaimer.IO;
using System.Diagnostics.CodeAnalysis;
using VertexChannel = System.Collections.Generic.IReadOnlyList<Reclaimer.Geometry.IVector>;

namespace Reclaimer.Geometry
{
    public class VertexBuffer
    {
        internal static readonly IEqualityComparer<VertexBuffer> EqualityComparer = new CustomEqualityComparer();

        public int Count => EnumerateChannels().Max(c => c?.Count) ?? default;

        public IList<VertexChannel> PositionChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> TextureCoordinateChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> NormalChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> TangentChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> BinormalChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> BlendIndexChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> BlendWeightChannels { get; } = new List<VertexChannel>();
        public IList<VertexChannel> ColorChannels { get; } = new List<VertexChannel>();

        public bool HasPositions => PositionChannels.Any(c => c?.Count > 0);
        public bool HasTextureCoordinates => TextureCoordinateChannels.Any(c => c?.Count > 0);
        public bool HasNormals => NormalChannels.Any(c => c?.Count > 0);
        public bool HasTangents => TangentChannels.Any(c => c?.Count > 0);
        public bool HasBinormals => BinormalChannels.Any(c => c?.Count > 0);
        public bool HasBlendIndices => BlendIndexChannels.Any(c => c?.Count > 0);
        public bool HasBlendWeights => BlendWeightChannels.Any(c => c?.Count > 0);
        public bool HasColors => ColorChannels.Any(c => c?.Count > 0);

        public bool HasImpliedBlendWeights { get; set; }

        public VertexBuffer Slice(int index, int count)
        {
            var result = new VertexBuffer();

            foreach (var (from, to) in EnumerateChannelSets().Zip(result.EnumerateChannelSets()))
            {
                foreach (var channel in from)
                {
                    if (channel is IVectorBuffer buffer)
                        to.Add(buffer.Slice(index, count));
                    else
                    {
                        var newList = new List<IVector>(count);
                        newList.AddRange(channel.GetSubset(index, count));
                        to.Add(newList);
                    }
                }
            }

            return result;
        }

        public void ReverseEndianness()
        {
            foreach (var buffer in EnumerateChannels().OfType<IVectorBuffer>())
                buffer.ReverseEndianness();
        }

        private IEnumerable<IList<VertexChannel>> EnumerateChannelSets()
        {
            yield return PositionChannels;
            yield return TextureCoordinateChannels;
            yield return NormalChannels;
            yield return TangentChannels;
            yield return BinormalChannels;
            yield return BlendIndexChannels;
            yield return BlendWeightChannels;
            yield return ColorChannels;
        }

        private IEnumerable<VertexChannel> EnumerateChannels() => EnumerateChannelSets().SelectMany(s => s);

        private sealed class CustomEqualityComparer : IEqualityComparer<VertexBuffer>
        {
            public bool Equals(VertexBuffer x, VertexBuffer y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                foreach (var (a, b) in x.EnumerateChannelSets().Zip(y.EnumerateChannelSets()))
                {
                    if (a.Count != b.Count)
                        return false;

                    if (ReferenceEquals(a, b) || (a.GetHashCode() == b.GetHashCode() && a.Equals(b)))
                        continue; //this set is equal, move to next

                    if (!a.Zip(b).All(t => CompareBuffers(t.First, t.Second)))
                        return false;
                }

                return true;
            }

            private static bool CompareBuffers(VertexChannel x, VertexChannel y)
            {
                //ReferenceEquals() || underlying type equals || IDataBuffer equals
                return ReferenceEquals(x, y) || (x.GetHashCode() == y.GetHashCode() && x.Equals(y))
                    || (x is IDataBuffer bx && y is IDataBuffer by && IDataBuffer.Equals(bx, by));
            }

            public int GetHashCode([DisallowNull] VertexBuffer obj)
            {
                var normals = HashCode.Combine(obj.NormalChannels.Count, obj.TangentChannels.Count, obj.BinormalChannels.Count);
                var blend = HashCode.Combine(obj.BlendIndexChannels.Count, obj.BlendWeightChannels.Count);
                return HashCode.Combine(obj.Count, obj.PositionChannels.Count, obj.TextureCoordinateChannels.Count, normals, blend, obj.ColorChannels.Count);
            }
        }
    }
}
