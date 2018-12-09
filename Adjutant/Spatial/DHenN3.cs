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
    /// Each dimension is limited to a minimum of -1 and a maximum of 1.
    /// The X dimension has 10 bits of precision, while the Y and Z dimensions have 11 bits of precision.
    /// </summary>
    public struct DHenN3 : IRealVector3D
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
                _value = (uint)((_value & ~0x3FF) | ((uint)value & 0x3FF));
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
                _value = (uint)((_value & ~(0x7FF << 10)) | (((uint)value & 0x7FF) << 10));
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
                _value = (uint)((_value & ~(0x7FF << 21)) | (((uint)value & 0x7FF) << 21));
            }
        }

        [CLSCompliant(false)]
        public DHenN3(uint value)
        {
            _value = value;
        }

        public DHenN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, -1, 1) * scaleX;
            y = Utils.Clamp(y, -1, 1) * scaleY;
            z = Utils.Clamp(z, -1, 1) * scaleZ;

            _value = (((uint)z & 0x7FF) << 21) |
                     (((uint)y & 0x7FF) << 10) |
                     ((uint)x & 0x3FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

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
