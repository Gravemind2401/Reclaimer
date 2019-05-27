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
    /// A collection of 4 byte values.
    /// </summary>
    public struct UByte4 : IXMVector
    {
        private uint bits;

        public byte X
        {
            get { return (byte)(bits & 0xFF); }
            set { bits = (uint)((bits & ~0xFF) | ((uint)value & 0xFF)); }
        }

        public byte Y
        {
            get { return (byte)((bits >> 8) & 0xFF); }
            set { bits = (uint)((bits & ~(0xFF << 8)) | (((uint)value & 0xFF) << 8)); }
        }

        public byte Z
        {
            get { return (byte)((bits >> 16) & 0xFF); }
            set { bits = (uint)((bits & ~(0xFF << 16)) | (((uint)value & 0xFF) << 16)); }
        }

        public byte W
        {
            get { return (byte)((bits >> 24) & 0xFF); }
            set { bits = (uint)((bits & ~(0xFF << 24)) | (((uint)value & 0xFF) << 24)); }
        }

        [CLSCompliant(false)]
        public UByte4(uint value)
        {
            bits = value;
        }

        public UByte4(byte x, byte y, byte z, byte w)
        {
            var temp = (w << 24) | (z << 16) | (y << 8) | x;
            bits = unchecked((uint)temp);
        }

        public float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public override string ToString() => Utils.CurrentCulture($"[{X}, {Y}, {Z}, {W}]");

        [CLSCompliant(false)]
        public static explicit operator uint(UByte4 value)
        {
            return value.bits;
        }

        [CLSCompliant(false)]
        public static explicit operator UByte4(uint value)
        {
            return new UByte4(value);
        }

        #region IXMVector

        float IXMVector.X
        {
            get { return X; }
            set { X = (byte)Utils.Clamp(value, 0, byte.MaxValue); }
        }

        float IXMVector.Y
        {
            get { return Y; }
            set { Y = (byte)Utils.Clamp(value, 0, byte.MaxValue); }
        }

        float IXMVector.Z
        {
            get { return Z; }
            set { Z = (byte)Utils.Clamp(value, 0, byte.MaxValue); }
        }

        float IXMVector.W
        {
            get { return W; }
            set { W = (byte)Utils.Clamp(value, 0, byte.MaxValue); }
        }

        VectorType IXMVector.VectorType => VectorType.UInt8_4;

        #endregion

        #region Equality Operators

        public static bool operator ==(UByte4 point1, UByte4 point2)
        {
            return point1.bits == point2.bits;
        }

        public static bool operator !=(UByte4 point1, UByte4 point2)
        {
            return !(point1 == point2);
        }

        public static bool Equals(UByte4 point1, UByte4 point2)
        {
            return point1.bits.Equals(point2.bits);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is UByte4))
                return false;

            return UByte4.Equals(this, (UByte4)obj);
        }

        public bool Equals(UByte4 value)
        {
            return UByte4.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return bits.GetHashCode();
        }

        #endregion
    }
}
