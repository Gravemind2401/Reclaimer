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
    [FixedSize(66)]
    [StructLayout(LayoutKind.Sequential)]
    public struct SkinnedVertex
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
        public byte NodeIndex1 { get; set; }

        [Offset(57)]
        public byte NodeIndex2 { get; set; }

        [Offset(58)]
        public RealVector2D NodeWeights { get; set; }

        public override string ToString() => Position.ToString();
    }
}
