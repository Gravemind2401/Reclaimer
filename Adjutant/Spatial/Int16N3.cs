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
    public struct Int16N3 : IXMVector
    {
        private short x, y, z;

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

        public float Z
        {
            get { return (z + short.MaxValue) / (float)ushort.MaxValue; }
            set { z = (short)(value * ushort.MaxValue - short.MaxValue); }
        }

        public Int16N3(short x, short y, short z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Int16N3(float x, float y, float z)
        {
            this.x = (short)(x * ushort.MaxValue - short.MaxValue);
            this.y = (short)(y * ushort.MaxValue - short.MaxValue);
            this.z = (short)(z * ushort.MaxValue - short.MaxValue);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        #region IXMVector

        float IXMVector.W
        {
            get { return float.NaN; }
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.Int16_N3;

        #endregion
    }
}
