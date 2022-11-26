using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    public class VertexBuffer
    {
        public int Count => EnumerateChannels().Max(c => c?.Count) ?? default;

        public IList<IVectorBuffer> PositionChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> TextureCoordinateChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> NormalChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> TangentChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BinormalChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BlendIndexChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> BlendWeightChannels { get; } = new List<IVectorBuffer>();
        public IList<IVectorBuffer> ColorChannels { get; } = new List<IVectorBuffer>();

        public bool HasPositions => PositionChannels.Any(c => c?.Count > 0);
        public bool HasTextureCoordinates => TextureCoordinateChannels.Any(c => c?.Count > 0);
        public bool HasNormals => NormalChannels.Any(c => c?.Count > 0);
        public bool HasTangents => TangentChannels.Any(c => c?.Count > 0);
        public bool HasBinormals => BinormalChannels.Any(c => c?.Count > 0);
        public bool HasBlendIndices => BlendIndexChannels.Any(c => c?.Count > 0);
        public bool HasBlendWeights => BlendWeightChannels.Any(c => c?.Count > 0);
        public bool HasColors => ColorChannels.Any(c => c?.Count > 0);

        public VertexBuffer GetSubset(int index, int count)
        {
            void CopySubsets(IList<IVectorBuffer> from, IList<IVectorBuffer> to)
            {
                foreach(var item in from)
                    to.Add(item.GetSubset(index, count));
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
            foreach (var buffer in EnumerateChannels())
                buffer?.SwapEndianness();
        }

        private IEnumerable<IVectorBuffer> EnumerateChannels()
        {
            return PositionChannels.Concat(NormalChannels).Concat(TangentChannels).Concat(BinormalChannels)
                .Concat(TextureCoordinateChannels).Concat(BlendIndexChannels).Concat(BlendWeightChannels).Concat(ColorChannels);
        }
    }
}
