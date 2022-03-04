using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 2-dimensional vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 16 bits of precision.
    /// </summary>
    public struct UInt16N2 : IXMVector
    {
        private UInt16N x, y;

        public float X
        {
            get { return x.Value; }
            set { x = new UInt16N(value); }
        }

        public float Y
        {
            get { return y.Value; }
            set { y = new UInt16N(value); }
        }

        public UInt16N2(ushort x, ushort y)
        {
            this.x = new UInt16N(x);
            this.y = new UInt16N(y);
        }

        public UInt16N2(float x, float y)
        {
            this.x = new UInt16N(x);
            this.y = new UInt16N(y);
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

        VectorType IXMVector.VectorType => VectorType.UInt16_N4;

        #endregion

        #region Equality Operators

        public static bool operator ==(UInt16N2 value1, UInt16N2 value2)
        {
            return value1.x == value2.x && value1.y == value2.y;
        }

        public static bool operator !=(UInt16N2 value1, UInt16N2 value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(UInt16N2 value1, UInt16N2 value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UInt16N2))
                return false;

            return UInt16N2.Equals(this, (UInt16N2)obj);
        }

        public bool Equals(UInt16N2 value)
        {
            return UInt16N2.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        #endregion
    }
}
