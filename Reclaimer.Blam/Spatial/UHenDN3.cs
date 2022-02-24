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
    /// The X and Y dimensions have 11 bits of precision, while the Z dimensions has 10 bits of precision.
    /// </summary>
    public struct UHenDN3 : IRealVector3D, IXMVector
    {
        private uint bits;

        private const float scaleXY = 0x7FF;
        private const float scaleZ = 0x3FF;

        public float X
        {
            get
            {
                return (bits & 0x7FF) / scaleXY;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleXY;
                bits = (uint)((bits & ~0x7FF) | ((uint)value & 0x7FF));
            }
        }

        public float Y
        {
            get
            {
                return ((bits >> 11) & 0x7FF) / scaleXY;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleXY;
                bits = (uint)((bits & ~(0x7FF << 11)) | (((uint)value & 0x7FF) << 11));
            }
        }

        public float Z
        {
            get
            {
                return ((bits >> 22) & 0x3FF) / scaleZ;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleZ;
                bits = (uint)((bits & ~(0x3FF << 22)) | (((uint)value & 0x3FF) << 22));
            }
        }

        public UHenDN3(uint value)
        {
            bits = value;
        }

        public UHenDN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, 0, 1) * scaleXY;
            y = Utils.Clamp(y, 0, 1) * scaleXY;
            z = Utils.Clamp(z, 0, 1) * scaleZ;

            bits = (((uint)z & 0x3FF) << 22) |
                   (((uint)y & 0x7FF) << 11) |
                    ((uint)x & 0x7FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        public static explicit operator uint(UHenDN3 value)
        {
            return value.bits;
        }

        public static explicit operator UHenDN3(uint value)
        {
            return new UHenDN3(value);
        }

        #region IXMVector

        float IXMVector.W
        {
            get { return float.NaN; }
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.UHenDN3;

        #endregion

        #region Equality Operators

        public static bool operator ==(UHenDN3 point1, UHenDN3 point2)
        {
            return point1.bits == point2.bits;
        }

        public static bool operator !=(UHenDN3 point1, UHenDN3 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UHenDN3 point1, UHenDN3 point2)
        {
            return point1.bits.Equals(point2.bits);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UHenDN3))
                return false;

            return UHenDN3.Equals(this, (UHenDN3)obj);
        }

        public bool Equals(UHenDN3 value)
        {
            return UHenDN3.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
        }

        #endregion
    }
}
