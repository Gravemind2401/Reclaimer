using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    //10/11/11
    public struct UDHenN3
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
                this._value = (uint)((this._value & ~0x3FF) | ((uint)value & 0x3FF));
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
                this._value = (uint)((this._value & ~(0x7FF << 10)) | (((uint)value & 0x7FF) << 10));
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
                this._value = (uint)((this._value & ~(0x7FF << 21)) | (((uint)value & 0x7FF) << 21));
            }
        }

        [CLSCompliant(false)]
        public UDHenN3(uint value)
        {
            this._value = value;
        }

        public UDHenN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, 0f, 1f) * scaleX;
            y = Utils.Clamp(x, 0f, 1f) * scaleY;
            z = Utils.Clamp(x, 0f, 1f) * scaleZ;

            _value = (((uint)z & 0x7FF) << 21) |
                    (((uint)y & 0x7FF) << 10) |
                    ((uint)x & 0x3FF);
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");
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
