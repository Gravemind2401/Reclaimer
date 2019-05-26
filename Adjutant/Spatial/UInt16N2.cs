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
            set { x = (ushort)(value * ushort.MaxValue); }
        }

        public float Y
        {
            get { return y / (float)ushort.MaxValue; }
            set { y = (ushort)(value * ushort.MaxValue); }
        }

        [CLSCompliant(false)]
        public UInt16N2(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
        }

        public UInt16N2(float x, float y)
        {
            this.x = (ushort)(x * ushort.MaxValue);
            this.y = (ushort)(y * ushort.MaxValue);
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
    }
}
