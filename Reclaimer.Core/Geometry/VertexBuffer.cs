using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VertexChannel = System.Collections.Generic.IReadOnlyList<Reclaimer.Geometry.IVector>;

namespace Reclaimer.Geometry
{
    public class VertexBuffer
    {
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

        public VertexBuffer Slice(int index, int count)
        {
            void CopySubsets(IList<VertexChannel> from, IList<VertexChannel> to)
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

            var result = new VertexBuffer();

            CopySubsets(PositionChannels, result.PositionChannels);
            CopySubsets(TextureCoordinateChannels, result.TextureCoordinateChannels);
            CopySubsets(NormalChannels, result.NormalChannels);
            CopySubsets(TangentChannels, result.TangentChannels);
            CopySubsets(BinormalChannels, result.BinormalChannels);
            CopySubsets(BlendIndexChannels, result.BlendIndexChannels);
            CopySubsets(BlendWeightChannels, result.BlendWeightChannels);
            CopySubsets(ColorChannels, result.ColorChannels);

            return result;
        }

        public void SwapEndianness()
        {
            foreach (var buffer in EnumerateChannels().OfType<IVectorBuffer>())
                buffer.SwapEndianness();
        }

        private IEnumerable<VertexChannel> EnumerateChannels()
        {
            return PositionChannels.Concat(NormalChannels).Concat(TangentChannels).Concat(BinormalChannels)
                .Concat(TextureCoordinateChannels).Concat(BlendIndexChannels).Concat(BlendWeightChannels).Concat(ColorChannels);
        }
    }
}
