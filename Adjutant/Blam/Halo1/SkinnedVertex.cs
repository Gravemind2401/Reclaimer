using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    [FixedSize(68)]
    public class SkinnedVertex : IVertex
    {
        [Offset(0)]
        public RealVector3D Position { get; set; }

        [Offset(12)]
        public RealVector3D Normal { get; set; }

        [Offset(24)]
        public RealVector3D Binormal { get; set; }

        [Offset(36)]
        public RealVector3D Tangent { get; set; }

        [Offset(48)]
        public RealVector2D TexCoords { get; set; }

        [Offset(56)]
        public short NodeIndex1 { get; set; }

        [Offset(58)]
        public short NodeIndex2 { get; set; }

        [Offset(60)]
        public RealVector2D NodeWeights { get; set; }

        public override string ToString() => Position.ToString();

        #region IVertex

        IReadOnlyList<IXMVector> IVertex.Position => new IXMVector[] { Position };

        IReadOnlyList<IXMVector> IVertex.TexCoords => new IXMVector[] { TexCoords };

        IReadOnlyList<IXMVector> IVertex.Normal => new IXMVector[] { Normal };

        IReadOnlyList<IXMVector> IVertex.Binormal => new IXMVector[] { Binormal };

        IReadOnlyList<IXMVector> IVertex.Tangent => new IXMVector[] { Tangent };

        IReadOnlyList<IXMVector> IVertex.BlendIndices => new IXMVector[] { new RealVector2D(NodeIndex1, NodeIndex2) };

        IReadOnlyList<IXMVector> IVertex.BlendWeight => new IXMVector[] { NodeWeights };

        IReadOnlyList<IXMVector> IVertex.Color => new IXMVector[0];

        #endregion
    }
}
