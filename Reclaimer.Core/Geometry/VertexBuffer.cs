using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Reclaimer.Geometry
{
    public class VertexBuffer
    {
        public int Count { get; }

        public IList<IVectorBuffer> PositionChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> NormalChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> TangentChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BinormalChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> TextureCoordinateChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BlendIndexChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BlendWeightChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> ColorChannels { get; } = new List<IVectorBuffer>();

        public bool HasPositions => PositionChannels.Any(c => c?.Count > 0);
        public bool HasNormals => NormalChannels.Any(c => c?.Count > 0);
        public bool HasTangents => TangentChannels.Any(c => c?.Count > 0);
        public bool HasBinormals => BinormalChannels.Any(c => c?.Count > 0);
        public bool HasTextureCoordinates => TextureCoordinateChannels.Any(c => c?.Count > 0);
        public bool HasBlendIndices => BlendIndexChannels.Any(c => c?.Count > 0);
        public bool HasBlendWeights => BlendWeightChannels.Any(c => c?.Count > 0);
        public bool HasColors => ColorChannels.Any(c => c?.Count > 0);

        public void SwapEndianness()
        {
            var channels = PositionChannels.Concat(NormalChannels).Concat(TangentChannels).Concat(BinormalChannels)
                .Concat(TextureCoordinateChannels).Concat(BlendIndexChannels).Concat(BlendWeightChannels).Concat(ColorChannels);

            foreach (var buffer in channels)
                buffer.SwapEndianness();
        }
    }
}
