using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional Vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// Each dimension has 8 bits of precision.
    /// </summary>
    public struct UByteN4 : IRealVector4D
    {
        private uint _value;

        private const float scale = 0xFF;

        public float X
        {
            get { return (_value & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (uint)((_value & ~0xFF) | ((uint)value & 0xFF));
            }
        }

        public float Y
        {
            get { return ((_value >> 8) & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (uint)((_value & ~(0xFF << 8)) | (((uint)value & 0xFF) << 8));
            }
        }

        public float Z
        {
            get { return ((_value >> 16) & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (uint)((_value & ~(0xFF << 16)) | (((uint)value & 0xFF) << 16));
            }
        }

        public float W
        {
            get { return ((_value >> 24) & 0xFF) / scale; }
            set
            {
                value = Utils.Clamp(value, 0, 1) * scale;
                _value = (uint)((_value & ~(0xFF << 24)) | (((uint)value & 0xFF) << 24));
            }
        }

        [CLSCompliant(false)]
        public UByteN4(uint value)
        {
            _value = value;
        }

        public UByteN4(byte x, byte y, byte z, byte w)
        {
            var temp = (z << 24) | (y << 16) | (y << 8) | x;
            _value = unchecked((uint)temp);
        }

        public UByteN4(float x, float y, float z, float w)
        {
            x = Utils.Clamp(x, 0, 1) * scale;
            y = Utils.Clamp(y, 0, 1) * scale;
            z = Utils.Clamp(z, 0, 1) * scale;
            w = Utils.Clamp(w, 0, 1) * scale;

            var temp = ((byte)z << 24) | ((byte)y << 16) | ((byte)y << 8) | (byte)x;
            _value = unchecked((uint)temp);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]");

        [CLSCompliant(false)]
        public static explicit operator uint(UByteN4 value)
        {
            return value._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UByteN4(uint value)
        {
            return new UByteN4(value);
        }

        #region Equality Operators

        public static bool operator ==(UByteN4 point1, UByteN4 point2)
        {
            return point1._value == point2._value;
        }

        public static bool operator !=(UByteN4 point1, UByteN4 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UByteN4 point1, UByteN4 point2)
        {
            return point1._value.Equals(point2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UByteN4))
                return false;

            return UByteN4.Equals(this, (UByteN4)obj);
        }

        public bool Equals(UByteN4 value)
        {
            return UByteN4.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
