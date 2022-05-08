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
    /// A 2-dimensional vector compressed into 16 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 8 bits of precision.
    /// </summary>
    public struct UByteN2 : IRealVector2D, IXMVector
    {
        private ushort bits;

        private const float scale = 0xFF;

        public float X
        {
            get => (bits & 0xFF) / scale;
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                bits = (ushort)((bits & ~0xFF) | ((ushort)value & 0xFF));
            }
        }

        public float Y
        {
            get => ((bits >> 8) & 0xFF) / scale;
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                bits = (ushort)((bits & ~(0xFF << 8)) | (((ushort)value & 0xFF) << 8));
            }
        }

        public UByteN2(ushort value)
        {
            bits = value;
        }

        public UByteN2(byte x, byte y)
        {
            var temp = (y << 8) | x;
            bits = (ushort)temp;
        }

        public UByteN2(float x, float y)
        {
            x = Utils.Clamp(x, 0, 1) * scale;
            y = Utils.Clamp(y, 0, 1) * scale;

            var temp = ((byte)y << 8) | (byte)x;
            bits = (ushort)temp;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}]");

        public static explicit operator ushort(UByteN2 value) => value.bits;
        public static explicit operator UByteN2(ushort value) => new UByteN2(value);

        #region IXMVector

        float IXMVector.Z
        {
            get => float.NaN;
            set { }
        }

        float IXMVector.W
        {
            get => float.NaN;
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.UInt8_N2;

        #endregion

        #region Equality Operators

        public static bool operator ==(UByteN2 value1, UByteN2 value2) => value1.bits == value2.bits;
        public static bool operator !=(UByteN2 value1, UByteN2 value2) => !(value1 == value2);

        public static bool Equals(UByteN2 value1, UByteN2 value2) => value1.bits.Equals(value2.bits);
        public override bool Equals(object obj)=> obj is UByteN2 value && UByteN2.Equals(this, value);
        public bool Equals(UByteN2 value) => UByteN2.Equals(this, value);

        public override int GetHashCode() => bits.GetHashCode();

        #endregion
    }
}
