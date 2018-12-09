using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 3-dimensional Vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// The X dimension has 10 bits of precision, while the Y and Z dimensions have 11 bits of precision.
    /// </summary>
    public struct UDHenN3 : IRealVector3D
    {
        private uint _value;

        private const float scaleX = 0x3FF;
        private const float scaleY = 0x7FF;
        private const float scaleZ = 0x7FF;

        public float X
        {
            get
            {
                return (_value & 0x3FF) / scaleX;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleX;
                _value = (uint)((_value & ~0x3FF) | ((uint)value & 0x3FF));
            }
        }

        public float Y
        {
            get
            {
                return ((_value >> 10) & 0x7FF) / scaleY;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleY;
                _value = (uint)((_value & ~(0x7FF << 10)) | (((uint)value & 0x7FF) << 10));
            }
        }

        public float Z
        {
            get
            {
                return ((_value >> 21) & 0x7FF) / scaleZ;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleZ;
                _value = (uint)((_value & ~(0x7FF << 21)) | (((uint)value & 0x7FF) << 21));
            }
        }

        [CLSCompliant(false)]
        public UDHenN3(uint value)
        {
            _value = value;
        }

        public UDHenN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, 0, 1) * scaleX;
            y = Utils.Clamp(y, 0, 1) * scaleY;
            z = Utils.Clamp(z, 0, 1) * scaleZ;

            _value = (((uint)z & 0x7FF) << 21) |
                     (((uint)y & 0x7FF) << 10) |
                     ((uint)x & 0x3FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        [CLSCompliant(false)]
        public static explicit operator uint(UDHenN3 value)
        {
            return value._value;
        }

        [CLSCompliant(false)]
        public static explicit operator UDHenN3(uint value)
        {
            return new UDHenN3(value);
        }

        #region Equality Operators

        public static bool operator ==(UDHenN3 point1, UDHenN3 point2)
        {
            return point1._value == point2._value;
        }

        public static bool operator !=(UDHenN3 point1, UDHenN3 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UDHenN3 point1, UDHenN3 point2)
        {
            return point1._value.Equals(point2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UDHenN3))
                return false;

            return UDHenN3.Equals(this, (UDHenN3)obj);
        }

        public bool Equals(UDHenN3 value)
        {
            return UDHenN3.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
