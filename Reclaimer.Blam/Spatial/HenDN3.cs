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
    /// Each dimension is limited to a minimum of -1 and a maximum of 1.
    /// The X and Y dimensions have 11 bits of precision, while the Z dimensions has 10 bits of precision.
    /// </summary>
    public struct HenDN3 : IRealVector3D, IXMVector
    {
        private uint bits;

        private const float scaleXY = 0x3FF;
        private const float scaleZ = 0x1FF;

        private static readonly uint[] SignExtendXY = { 0x00000000, 0xFFFFF800 };
        private static readonly uint[] SignExtendZ = { 0x00000000, 0xFFFFFC00 };

        public float X
        {
            get
            {
                var temp = bits & 0x7FF;
                return (short)(temp | SignExtendXY[temp >> 10]) / scaleXY;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleXY;
                bits = (uint)((bits & ~0x7FF) | ((uint)value & 0x7FF));
            }
        }

        public float Y
        {
            get
            {
                var temp = (bits >> 11) & 0x7FF;
                return (short)(temp | SignExtendXY[temp >> 10]) / scaleXY;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleXY;
                bits = (uint)((bits & ~(0x7FF << 11)) | (((uint)value & 0x7FF) << 11));
            }
        }

        public float Z
        {
            get
            {
                var temp = (bits >> 22) & 0x3FF;
                return (short)(temp | SignExtendZ[temp >> 9]) / scaleZ;
            }
            set
            {
                value = Utils.Clamp(value, -1f, 1f) * scaleZ;
                bits = (uint)((bits & ~(0x3FF << 22)) | (((uint)value & 0x3FF) << 22));
            }
        }

        public HenDN3(uint value)
        {
            bits = value;
        }

        public HenDN3(float x, float y, float z)
        {
            x = Utils.Clamp(x, -1, 1) * scaleXY;
            y = Utils.Clamp(y, -1, 1) * scaleXY;
            z = Utils.Clamp(z, -1, 1) * scaleZ;

            bits = (((uint)z & 0x3FF) << 22) |
                   (((uint)y & 0x7FF) << 11) |
                    ((uint)x & 0x7FF);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public override string ToString() => Utils.CurrentCulture($"[{X:F6}, {Y:F6}, {Z:F6}]");

        public static explicit operator uint(HenDN3 value) => value.bits;
        public static explicit operator HenDN3(uint value) => new HenDN3(value);

        #region IXMVector

        float IXMVector.W
        {
            get => float.NaN;
            set { }
        }

        VectorType IXMVector.VectorType => VectorType.HenDN3;

        #endregion

        #region Equality Operators

        public static bool operator ==(HenDN3 value1, HenDN3 value2) => value1.bits == value2.bits;
        public static bool operator !=(HenDN3 value1, HenDN3 value2) => !(value1 == value2);

        public static bool Equals(HenDN3 value1, HenDN3 value2) => value1.bits.Equals(value2.bits);
        public override bool Equals(object obj)=> obj is HenDN3 value && HenDN3.Equals(this, value);
        public bool Equals(HenDN3 value) => HenDN3.Equals(this, value);

        public override int GetHashCode() => bits.GetHashCode();

        #endregion
    }
}
