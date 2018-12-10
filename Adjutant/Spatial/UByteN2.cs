using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 2-dimensional Vector compressed into 16 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 8 bits of precision.
    /// </summary>
    public struct UByteN2 : IRealVector2D
    {
        private ushort _value;

        private const float scale = 0xFF;

        public float X
        {
            get { return (_value & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (ushort)((_value & ~0xFF) | ((ushort)value & 0xFF));
            }
        }

        public float Y
        {
            get { return ((_value >> 8) & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (ushort)((_value & ~(0xFF << 8)) | (((ushort)value & 0xFF) << 8));
            }
        }

        [CLSCompliant(false)]
        public UByteN2(ushort value)
        {
            _value = value;
        }

        public UByteN2(byte x, byte y)
        {
            var temp = (y << 8) | x;
            _value = (ushort)temp;
        }

        public UByteN2(float x, float y)
        {
            x = Utils.Clamp(x, 0, 1) * scale;
            y = Utils.Clamp(y, 0, 1) * scale;

            var temp = ((byte)y << 8) | (byte)x;
            _value = (ushort)temp;
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}]");

        [CLSCompliant(false)]
        public static explicit operator ushort(UByteN2 value)
        {
            return value._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UByteN2(ushort value)
        {
            return new UByteN2(value);
        }

        #region Equality Operators

        public static bool operator ==(UByteN2 point1, UByteN2 point2)
        {
            return point1._value == point2._value;
        }

        public static bool operator !=(UByteN2 point1, UByteN2 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UByteN2 point1, UByteN2 point2)
        {
            return point1._value.Equals(point2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UByteN2))
                return false;

            return UByteN2.Equals(this, (UByteN2)obj);
        }

        public bool Equals(UByteN2 value)
        {
            return UByteN2.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
