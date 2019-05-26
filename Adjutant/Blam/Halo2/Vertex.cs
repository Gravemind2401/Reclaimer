using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class Vertex : IVertex
    {
        public Int16N3 Position { get; set; }

        public Int16N2 TexCoords { get; set; }

        public HenDN3 Normal { get; set; }

        #region IVertex

        IReadOnlyList<IXMVector> IVertex.Position => new IXMVector[] { Position };

        IReadOnlyList<IXMVector> IVertex.TexCoords => new IXMVector[] { TexCoords };

        IReadOnlyList<IXMVector> IVertex.Normal => new IXMVector[] { Normal };

        IReadOnlyList<IXMVector> IVertex.Binormal => new IXMVector[0];

        IReadOnlyList<IXMVector> IVertex.Tangent => new IXMVector[0];

        IReadOnlyList<IXMVector> IVertex.BlendIndices => new IXMVector[0];// { new RealVector2D(NodeIndex1, NodeIndex2) };

        IReadOnlyList<IXMVector> IVertex.BlendWeight => new IXMVector[0];// { NodeWeights };

        IReadOnlyList<IXMVector> IVertex.Color => new IXMVector[0];

        #endregion
    }
}
