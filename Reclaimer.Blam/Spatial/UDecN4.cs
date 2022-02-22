using Adjutant.Geometry;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Spatial
{
    /// <summary>
    /// A 4-dimensional vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of 0 and a maximum of 1.
    /// The X, Y and Z dimensions each have 10 bits of precision, while the W dimension has 3 bits of precision.
    /// </summary>
    public struct UDecN4 : IRealVector4D, IXMVector
    {
        private uint bits;

        private const float scale = 0x3FF;
        private const float scaleW = 0x003;

        public float X
        {
            get
            {
                return (bits & 0x3FF) / scale;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scale;
                bits = (uint)((bits & ~0x3FF) | ((uint)value & 0x3FF));
            }
        }

        public float Y
        {
            get
            {
                return ((bits >> 10) & 0x3FF) / scale;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scale;
                bits = (uint)((bits & ~(0x3FF << 10)) | (((uint)value & 0x3FF) << 10));
            }
        }

        public float Z
        {
            get
            {
                return ((bits >> 20) & 0x3FF) / scale;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scale;
                bits = (uint)((bits & ~(0x3FF << 20)) | (((uint)value & 0x3FF) << 20));
            }
        }

        public float W
        {
            get
            {
                return ((bits >> 30) & 0x003) / scaleW;
            }
            set
            {
                value = Utils.Clamp(value, 0f, 1f) * scaleW;
                bits = (uint)((bits & ~(0x003 << 30)) | (((uint)value & 0x003) << 30));
            }
        }

        public UDecN4(uint value)
        {
            bits = value;
        }

        public UDecN4(float x, float y, float z, float w)
        {
            x = Utils.Clamp(x, 0, 1) * scale;
            y = Utils.Clamp(y, 0, 1) * scale;
            z = Utils.Clamp(z, 0, 1) * scale;
            w = Utils.Clamp(w, 0, 1) * scaleW;

            bits = (((uint)w & 0x003) << 30) |
                   (((uint)z & 0x3FF) << 20) |
                   (((uint)y & 0x3FF) << 10) |
                    ((uint)x & 0x3FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F3}]");

        public static explicit operator uint(UDecN4 value)
        {
            return value.bits;
        }

        public static explicit operator UDecN4(uint value)
        {
            return new UDecN4(value);
        }

        #region IXMVector

        VectorType IXMVector.VectorType => VectorType.UDecN4;

        #endregion

        #region Equality Operators

        public static bool operator ==(UDecN4 point1, UDecN4 point2)
        {
            return point1.bits == point2.bits;
        }

        public static bool operator !=(UDecN4 point1, UDecN4 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UDecN4 point1, UDecN4 point2)
        {
            return point1.bits.Equals(point2.bits);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UDecN4))
                return false;

            return UDecN4.Equals(this, (UDecN4)obj);
        }

        public bool Equals(UDecN4 value)
        {
            return UDecN4.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
        }

        #endregion
    }
}
