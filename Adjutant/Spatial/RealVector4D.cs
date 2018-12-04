using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(16)]
    public struct RealVector4D : IRealVector4D
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

        public float Length => (float)Math.Sqrt(x * x + y * y + z * z + w * w);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");
       
        #region Equality Operators

        public static bool operator ==(RealVector4D point1, RealVector4D point2)
        {
            return point1.X == point2.X &&
                   point1.Y == point2.Y &&
                   point1.Z == point2.Z &&
                   point1.W == point2.W;
        }

        public static bool operator !=(RealVector4D point1, RealVector4D point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(RealVector4D point1, RealVector4D point2)
        {
            return point1.X.Equals(point2.X) &&
                   point1.Y.Equals(point2.Y) &&
                   point1.Z.Equals(point2.Z) &&
                   point1.W.Equals(point2.W);
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
            return X.GetHashCode() ^
                   Y.GetHashCode() ^
                   Z.GetHashCode() ^
                   W.GetHashCode();
        }

        #endregion
    }
}
