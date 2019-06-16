using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Geometry
{
    public struct CompressedVertex : IVertex
    {
        private readonly IVertex source;
        private readonly IRealBounds5D bounds;

        public IReadOnlyList<IXMVector> Position { get; }
        public IReadOnlyList<IXMVector> TexCoords { get; }
        public IReadOnlyList<IXMVector> Normal { get; }
        public IReadOnlyList<IXMVector> Binormal { get; }
        public IReadOnlyList<IXMVector> Tangent { get; }
        public IReadOnlyList<IXMVector> BlendIndices => source.BlendIndices;
        public IReadOnlyList<IXMVector> BlendWeight => source.BlendWeight;
        public IReadOnlyList<IXMVector> Color => source.Color;

        public CompressedVertex(IVertex source, IRealBounds5D bounds)
        {
            this.source = source;
            this.bounds = bounds;

            var transform3D = bounds.AsTransform();
            var transform2D = bounds.AsTextureTransform();

            Position = source.Position.Select(v => (IXMVector)new TransformedVector(v, transform3D, true)).ToArray();
            TexCoords = source.TexCoords.Select(v => (IXMVector)new TransformedVector(v, transform2D, true)).ToArray();
            Normal = source.Normal.Select(v => (IXMVector)new TransformedVector(v, transform3D, false)).ToArray();
            Binormal = source.Binormal.Select(v => (IXMVector)new TransformedVector(v, transform3D, false)).ToArray();
            Tangent = source.Tangent.Select(v => (IXMVector)new TransformedVector(v, transform3D, false)).ToArray();
        }
    }
}
