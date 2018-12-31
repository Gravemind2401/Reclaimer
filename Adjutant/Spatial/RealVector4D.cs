using Adjutant.Geometry;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector4D : IRealVector4D, IXMVector
    {
        private float x, y, z, w;

        [Offset(0)]
        public float X
        {
            get { return x; }
            set { x = value; }
        }

        [Offset(4)]
        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        [Offset(8)]
        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        [Offset(12)]
        public float W
        {
            get { return w; }
            set { w = value; }
        }

        public RealVector4D(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");

        #region IXMVector

        VectorType IXMVector.VectorType => VectorType.Float32_4;

        #endregion
    }
}
