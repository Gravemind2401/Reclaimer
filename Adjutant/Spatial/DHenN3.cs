using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    //10/11/11
    public struct DHenN3
    {
        private uint _value;

        private const float scaleX = 0x1FF;
        private const float scaleY = 0x3FF;
        private const float scaleZ = 0x3FF;

        private static uint[] SignExtendX = { 0x00000000, 0xFFFFFC00 };
        private static uint[] SignExtendYZ = { 0x00000000, 0xFFFFF800 };

        public float X
        {
            get
            {
                var temp = _value & 0x3FF;
                return (short)(temp | SignExtendX[temp >> 9]) / scaleX;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleX;
                this._value = (uint)((this._value & ~0x3FF) | ((uint)value & 0x3FF));
            }
        }

        public float Y
        {
            get
            {
                var temp = (_value >> 10) & 0x7FF;
                return (short)(temp | SignExtendYZ[temp >> 10]) / scaleY;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleY;
                this._value = (uint)((this._value & ~(0x7FF << 10)) | (((uint)value & 0x7FF) << 10));
            }
        }

        public float Z
        {
            get
            {
                var temp = (_value >> 21) & 0x7FF;
                return (short)(temp | SignExtendYZ[temp >> 10]) / scaleZ;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleZ;
                this._value = (uint)((this._value & ~(0x7FF << 21)) | (((uint)value & 0x7FF) << 21));
            }
        }
        
        [CLSCompliant(false)]
        public DHenN3(uint value)
        {
            this._value = value;
        }

        public DHenN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, -1f, 1f) * scaleX;
            y = Utils.Clamp(x, -1f, 1f) * scaleY;
            z = Utils.Clamp(x, -1f, 1f) * scaleZ;

            _value = (((uint)z & 0x7FF) << 21) |
                    (((uint)y & 0x7FF) << 10) |
                    ((uint)x & 0x3FF);
        }

        public override string ToString()
        {
            return Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");
        }

        #region Equality Operators

        public static bool operator ==(DHenN3 point1, DHenN3 point2)
        {
            return point1._value == point2._value;
        }

        public static bool operator !=(DHenN3 point1, DHenN3 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(DHenN3 point1, DHenN3 point2)
        {
            return point1._value.Equals(point2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is DHenN3))
                return false;

            return DHenN3.Equals(this, (DHenN3)obj);
        }

        public bool Equals(DHenN3 value)
        {
            return DHenN3.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion
    }
}
