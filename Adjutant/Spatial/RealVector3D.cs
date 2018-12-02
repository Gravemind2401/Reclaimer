using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    [FixedSize(12)]
    public struct RealVector3D : IRealVector3D
    {
        private float x;
        private float y;
        private float z;

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

        public float Length
        {
            get { return (float)Math.Sqrt(x * x + y * y + z * z); }
        }

        #region Equality Operators

        public static bool operator ==(RealVector3D point1, RealVector3D point2)
        {
            return point1.X == point2.X &&
                   point1.Y == point2.Y &&
                   point1.Z == point2.Z;
        }

        public static bool operator !=(RealVector3D point1, RealVector3D point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(RealVector3D point1, RealVector3D point2)
        {
            return point1.X.Equals(point2.X) &&
                   point1.Y.Equals(point2.Y) &&
                   point1.Z.Equals(point2.Z);
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
            return X.GetHashCode() ^
                   Y.GetHashCode() ^
                   Z.GetHashCode();
        }

        #endregion
    }
}
