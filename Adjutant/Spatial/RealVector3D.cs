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
    /// A 3-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(12)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector3D : IRealVector3D, IXMVector
    {
        private float x, y, z;

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

        public RealVector3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        #region IXMVector

        float IXMVector.W
        {
            get { return float.NaN; }
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.Float32_3;

        #endregion

        #region Equality Operators

        public static bool operator ==(RealVector3D value1, RealVector3D value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z;
        }

        public static bool operator !=(RealVector3D value1, RealVector3D value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(RealVector3D value1, RealVector3D value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y) && value1.z.Equals(value2.z);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is RealVector3D))
                return false;

            return RealVector3D.Equals(this, (RealVector3D)obj);
        }

        public bool Equals(RealVector3D value)
        {
            return RealVector3D.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        #endregion
    }
}
