using Adjutant.Geometry;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    public struct Int16N4 : IXMVector
    {
        private Int16N x, y, z, w;

        public float X
        {
            get => x.Value;
            set => x = new Int16N(value);
        }

        public float Y
        {
            get => y.Value;
            set => y = new Int16N(value);
        }

        public float Z
        {
            get => z.Value;
            set => z = new Int16N(value);
        }

        public float W
        {
            get => w.Value;
            set => w = new Int16N(value);
        }

        public Int16N4(short x, short y, short z, short w)
        {
            this.x = new Int16N(x);
            this.y = new Int16N(y);
            this.z = new Int16N(z);
            this.w = new Int16N(w);
        }

        public Int16N4(float x, float y, float z, float w)
        {
            this.x = new Int16N(x);
            this.y = new Int16N(y);
            this.z = new Int16N(z);
            this.w = new Int16N(w);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");

        #region IXMVector

        VectorType IXMVector.VectorType => VectorType.Int16_N4;

        #endregion

        #region Equality Operators

        public static bool operator ==(Int16N4 value1, Int16N4 value2)
        {
            return value1.x == value2.x && value1.y == value2.y && value1.z == value2.z && value1.w == value2.w;
        }

        public static bool operator !=(Int16N4 value1, Int16N4 value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(Int16N4 value1, Int16N4 value2)
        {
            return value1.x.Equals(value2.x) && value1.y.Equals(value2.y) && value1.z.Equals(value2.z) && value1.w.Equals(value2.w);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is Int16N4))
                return false;

            return Int16N4.Equals(this, (Int16N4)obj);
        }

        public bool Equals(Int16N4 value)
        {
            return Int16N4.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        #endregion
    }
}
