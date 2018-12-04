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
    /// A 2-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(8)]
    public struct RealVector2D : IRealVector2D
    {
        private float x, y;

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

        public RealVector2D(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float Length => (float)Math.Sqrt(x * x + y * y);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}]");
       
        #region Equality Operators

        public static bool operator ==(RealVector2D point1, RealVector2D point2)
        {
            return point1.X == point2.X &&
                   point1.Y == point2.Y;
        }

        public static bool operator !=(RealVector2D point1, RealVector2D point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(RealVector2D point1, RealVector2D point2)
        {
            return point1.X.Equals(point2.X) &&
                   point1.Y.Equals(point2.Y);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is RealVector2D))
                return false;

            return RealVector2D.Equals(this, (RealVector2D)obj);
        }

        public bool Equals(RealVector2D value)
        {
            return RealVector2D.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^
                   Y.GetHashCode();
        }

        #endregion
    }
}
