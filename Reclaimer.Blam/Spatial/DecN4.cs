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
    /// A 4-dimensional vector compressed into 32 bits.
    /// Each dimension is limited to a minimum of -1 and a maximum of 1.
    /// The X, Y and Z dimensions each have 10 bits of precision, while the W dimension has 3 bits of precision.
    /// </summary>
    public struct DecN4 : IRealVector4D, IXMVector
    {
        private uint bits;

        private const float scale = 0x1FF;
        private const float scaleW = 0x001;

        private static readonly uint[] SignExtend = { 0x00000000, 0xFFFFFC00 };
        private static readonly uint[] SignExtendW = { 0x00000000, 0xFFFFFFFC };

        public float X
        {
            get
            {
                var temp = bits & 0x3FF;
                return (short)(temp | SignExtend[temp >> 9]) / scale;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scale;
                bits = (uint)((bits & ~0x3FF) | ((uint)value & 0x3FF));
            }
        }

        public float Y
        {
            get
            {
                var temp = (bits >> 10) & 0x3FF;
                return (short)(temp | SignExtend[temp >> 9]) / scale;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scale;
                bits = (uint)((bits & ~(0x3FF << 10)) | (((uint)value & 0x3FF) << 10));
            }
        }

        public float Z
        {
            get
            {
                var temp = (bits >> 20) & 0x3FF;
                return (short)(temp | SignExtend[temp >> 9]) / scale;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scale;
                bits = (uint)((bits & ~(0x3FF << 20)) | (((uint)value & 0x3FF) << 20));
            }
        }

        public float W
        {
            get
            {
                var temp = (bits >> 30) & 0x003;
                return (short)(temp | SignExtendW[temp >> 1]) / scaleW;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleW;
                bits = (uint)((bits & ~(0x003 << 30)) | (((uint)value & 0x003) << 30));
            }
        }

        public DecN4(uint value)
        {
            bits = value;
        }

        public DecN4(float x, float y, float z, float w)
        {
            x = Utils.Clamp(x, -1, 1) * scale;
            y = Utils.Clamp(y, -1, 1) * scale;
            z = Utils.Clamp(z, -1, 1) * scale;
            w = Utils.Clamp(w, -1, 1) * scaleW;

            bits = (((uint)w & 0x003) << 30) |
                   (((uint)z & 0x3FF) << 20) |
                   (((uint)y & 0x3FF) << 10) |
                    ((uint)x & 0x3FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}, {W:F0}]");

        public static explicit operator uint(DecN4 value) => value.bits;
        public static explicit operator DecN4(uint value) => new DecN4(value);

        #region IXMVector

        VectorType IXMVector.VectorType => VectorType.DecN4;

        #endregion

        #region Equality Operators

        public static bool operator ==(DecN4 value1, DecN4 value2) => value1.bits == value2.bits;
        public static bool operator !=(DecN4 value1, DecN4 value2) => !(value1 == value2);

        public static bool Equals(DecN4 value1, DecN4 value2) => value1.bits.Equals(value2.bits);
        public override bool Equals(object obj)=> obj is DecN4 value && DecN4.Equals(this, value);
        public bool Equals(DecN4 value) => DecN4.Equals(this, value);

        public override int GetHashCode() => bits.GetHashCode();

        #endregion
    }
}
