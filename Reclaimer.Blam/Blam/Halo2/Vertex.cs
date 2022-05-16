using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo2
{
    public class Vertex : IVertex
    {
        public UInt16N4 Position { get; set; }

        public UInt16N2 TexCoords { get; set; }

        public HenDN3 Normal { get; set; }

        public RealVector4D BlendIndices { get; set; }

        public RealVector4D BlendWeight { get; set; }

        #region IVertex

        IReadOnlyList<IXMVector> IVertex.Position => new IXMVector[] { Position };

        IReadOnlyList<IXMVector> IVertex.TexCoords => new IXMVector[] { TexCoords };

        IReadOnlyList<IXMVector> IVertex.Normal => new IXMVector[] { Normal };

        IReadOnlyList<IXMVector> IVertex.Binormal => Array.Empty<IXMVector>();

        IReadOnlyList<IXMVector> IVertex.Tangent => Array.Empty<IXMVector>();

        IReadOnlyList<IXMVector> IVertex.BlendIndices => new IXMVector[] { BlendIndices };

        IReadOnlyList<IXMVector> IVertex.BlendWeight => new IXMVector[] { BlendWeight };

        IReadOnlyList<IXMVector> IVertex.Color => Array.Empty<IXMVector>();

        #endregion
    }

    public class WorldVertex : IVertex
    {
        public RealVector3D Position { get; set; }

        public RealVector2D TexCoords { get; set; }

        public HenDN3 Normal { get; set; }

        #region IVertex

        IReadOnlyList<IXMVector> IVertex.Position => new IXMVector[] { Position };

        IReadOnlyList<IXMVector> IVertex.TexCoords => new IXMVector[] { TexCoords };

        IReadOnlyList<IXMVector> IVertex.Normal => new IXMVector[] { Normal };

        IReadOnlyList<IXMVector> IVertex.Binormal => Array.Empty<IXMVector>();

        IReadOnlyList<IXMVector> IVertex.Tangent => Array.Empty<IXMVector>();

        IReadOnlyList<IXMVector> IVertex.BlendIndices => Array.Empty<IXMVector>();// { new RealVector2D(NodeIndex1, NodeIndex2) };

        IReadOnlyList<IXMVector> IVertex.BlendWeight => Array.Empty<IXMVector>();// { NodeWeights };

        IReadOnlyList<IXMVector> IVertex.Color => Array.Empty<IXMVector>();

        #endregion
    }
}
