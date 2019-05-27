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

        #region Equality Operators

        public static bool operator ==(RealVector4D value1, RealVector4D value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z && value1.w == value2.w;
        }

        public static bool operator !=(RealVector4D value1, RealVector4D value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(RealVector4D value1, RealVector4D value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y) && value1.z.Equals(value2.z) && value1.w.Equals(value2.w);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is RealVector4D))
                return false;

            return RealVector4D.Equals(this, (RealVector4D)obj);
        }

        public bool Equals(RealVector4D value)
        {
            return RealVector4D.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        #endregion
    }
}
