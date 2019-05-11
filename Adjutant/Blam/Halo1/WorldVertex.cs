using Adjutant.Geometry;
using Adjutant.Spatial;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    [FixedSize(56)]
    public class WorldVertex : IVertex
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

        public override string ToString() => Position.ToString();

        #region IVertex

        IXMVector[] IVertex.Position => new IXMVector[] { Position };

        IXMVector[] IVertex.TexCoords => new IXMVector[] { TexCoords };

        IXMVector[] IVertex.Normal => new IXMVector[] { Normal };

        IXMVector[] IVertex.Binormal => new IXMVector[] { Binormal };

        IXMVector[] IVertex.Tangent => new IXMVector[] { Tangent };

        IXMVector[] IVertex.BlendIndices => new IXMVector[0];

        IXMVector[] IVertex.BlendWeight => new IXMVector[0];

        IXMVector[] IVertex.Color => new IXMVector[0];

        #endregion
    }
}
