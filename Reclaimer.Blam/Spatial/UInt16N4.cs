using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using Reclaimer.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional vector compressed into 64 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 16 bits of precision.
    /// </summary>
    public struct UInt16N4 : IXMVector
    {
        private UInt16N x, y, z, w;

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

        public float Z
        {
            get { return z.Value; }
            set { z = new UInt16N(value); }
        }

        public float W
        {
            get { return w.Value; }
            set { w = new UInt16N(value); }
        }

        public UInt16N4(ushort x, ushort y, ushort z, ushort w)
        {
            this.x = new UInt16N(x);
            this.y = new UInt16N(y);
            this.z = new UInt16N(z);
            this.w = new UInt16N(w);
        }

        public UInt16N4(float x, float y, float z, float w)
        {
            this.x = new UInt16N(x);
            this.y = new UInt16N(y);
            this.z = new UInt16N(z);
            this.w = new UInt16N(w);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");

        #region IXMVector

        VectorType IXMVector.VectorType => VectorType.UInt16_N4;

        #endregion

        #region Equality Operators

        public static bool operator ==(UInt16N4 value1, UInt16N4 value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z && value1.w == value2.w;
        }

        public static bool operator !=(UInt16N4 value1, UInt16N4 value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(UInt16N4 value1, UInt16N4 value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y) && value1.z.Equals(value2.z) && value1.w.Equals(value2.w);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UInt16N4))
                return false;

            return UInt16N4.Equals(this, (UInt16N4)obj);
        }

        public bool Equals(UInt16N4 value)
        {
            return UInt16N4.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        #endregion
    }
}
