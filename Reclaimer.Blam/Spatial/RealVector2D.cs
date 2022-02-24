using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using Reclaimer.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 2-dimensional vector.
    /// Each dimension is represented by a 32-bit floating point number.
    /// </summary>
    [FixedSize(8)]
    [StructLayout(LayoutKind.Sequential)]
    public struct RealVector2D : IRealVector2D, IXMVector
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

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}]");

        #region IXMVector

        float IXMVector.Z
        {
            get { return float.NaN; }
            set { }
        }

        float IXMVector.W
        {
            get { return float.NaN; }
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.Float32_2;

        #endregion

        #region Equality Operators

        public static bool operator ==(RealVector2D value1, RealVector2D value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

        public static bool operator !=(RealVector2D value1, RealVector2D value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(RealVector2D value1, RealVector2D value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y);
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
            return x.GetHashCode() ^ y.GetHashCode();
        }

        #endregion
    }
}
