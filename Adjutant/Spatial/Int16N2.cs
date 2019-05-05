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
    public struct Int16N2 : IXMVector
    {
        private short x, y;

        public float X
        {
            get { return (x + short.MaxValue) / (float)ushort.MaxValue; }
            set { x = (short)(value * ushort.MaxValue - short.MaxValue); }
        }

        public float Y
        {
            get { return (y + short.MaxValue) / (float)ushort.MaxValue; }
            set { y = (short)(value * ushort.MaxValue - short.MaxValue); }
        }

        public Int16N2(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public Int16N2(float x, float y)
        {
            this.x = (short)(x * ushort.MaxValue - short.MaxValue);
            this.y = (short)(y * ushort.MaxValue - short.MaxValue);
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

        VectorType IXMVector.VectorType => VectorType.Int16_N2;

        #endregion
    }
}
