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
    /// A 3-dimensional vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// The X dimension has 10 bits of precision, while the Y and Z dimensions have 11 bits of precision.
    /// </summary>
    public struct UDHenN3 : IRealVector3D, IXMVector
    {
        private uint bits;

        private const float scaleX = 0x3FF;
        private const float scaleYZ = 0x7FF;

        public float X
        {
            get => (bits & 0x3FF) / scaleX;
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleX;
                bits = (uint)((bits & ~0x3FF) | ((uint)value & 0x3FF));
            }
        }

        public float Y
        {
            get => ((bits >> 10) & 0x7FF) / scaleYZ;
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleYZ;
                bits = (uint)((bits & ~(0x7FF << 10)) | (((uint)value & 0x7FF) << 10));
            }
        }

        public float Z
        {
            get => ((bits >> 21) & 0x7FF) / scaleYZ;
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleYZ;
                bits = (uint)((bits & ~(0x7FF << 21)) | (((uint)value & 0x7FF) << 21));
            }
        }

        public UDHenN3(uint value)
        {
            bits = value;
        }

        public UDHenN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, 0, 1) * scaleX;
            y = Utils.Clamp(y, 0, 1) * scaleYZ;
            z = Utils.Clamp(z, 0, 1) * scaleYZ;

            bits = (((uint)z & 0x7FF) << 21) |
                   (((uint)y & 0x7FF) << 10) |
                    ((uint)x & 0x3FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        public static explicit operator uint(UDHenN3 value)
        {
            return value.bits;
        }

        public static explicit operator UDHenN3(uint value)
        {
            return new UDHenN3(value);
        }

        #region IXMVector

        float IXMVector.W
        {
            get => float.NaN;
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.UDHenN3;

        #endregion

        #region Equality Operators

        public static bool operator ==(UDHenN3 point1, UDHenN3 point2)
        {
            return point1.bits == point2.bits;
        }

        public static bool operator !=(UDHenN3 point1, UDHenN3 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UDHenN3 point1, UDHenN3 point2)
        {
            return point1.bits.Equals(point2.bits);
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
            return bits.GetHashCode();
        }

        #endregion
    }
}
