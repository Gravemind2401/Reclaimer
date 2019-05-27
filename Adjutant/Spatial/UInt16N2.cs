using Adjutant.Geometry;
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
    /// A 2-dimensional vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 16 bits of precision.
    /// </summary>
    public struct UInt16N2 : IXMVector
    {
        private ushort x, y;

        public float X
        {
            get { return x / (float)ushort.MaxValue; }
            set { x = (ushort)(Utils.Clamp(value, 0, 1) * ushort.MaxValue); }
        }

        public float Y
        {
            get { return y / (float)ushort.MaxValue; }
            set { y = (ushort)(Utils.Clamp(value, 0, 1) * ushort.MaxValue); }
        }

        [CLSCompliant(false)]
        public UInt16N2(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
        }

        public UInt16N2(float x, float y)
        {
            this.x = (ushort)(Utils.Clamp(x, 0, 1) * ushort.MaxValue);
            this.y = (ushort)(Utils.Clamp(y, 0, 1) * ushort.MaxValue);
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
