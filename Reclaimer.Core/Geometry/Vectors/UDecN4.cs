using System;
using System.Numerics;

// This file was automatically generated via the 'PackedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector packed into 32 bits.
    /// Each axis has a precision of 10, 10, 10, 2 bits respectively.
    /// Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("PackedVectors.tt", "")]    
    public struct UDecN4 : IEquatable<UDecN4>, IVector4, IReadOnlyVector4
    {
        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(10, 10, 10, 2);

        private uint bits;

        #region Axis Properties

        public float X
        {
            get => helper.GetX(in bits);
            set => helper.SetX(ref bits, value);
        }

        public float Y
        {
            get => helper.GetY(in bits);
            set => helper.SetY(ref bits, value);
        }

        public float Z
        {
            get => helper.GetZ(in bits);
            set => helper.SetZ(ref bits, value);
        }

        public float W
        {
            get => helper.GetW(in bits);
            set => helper.SetW(ref bits, value);
        }

        #endregion

        public UDecN4(uint value)
        {
            bits = value;
        }

        public UDecN4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        public UDecN4(float x, float y, float z, float w)
        {
            bits = default;
            (X, Y, Z, W) = (x, y, z, w);
        }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region Cast Operators

        public static explicit operator Vector4(UDecN4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator UDecN4(Vector4 value) => new UDecN4(value);

        public static explicit operator uint(UDecN4 value) => value.bits;
        public static explicit operator UDecN4(uint value) => new UDecN4(value);

        #endregion

        #region Equality Operators

        public static bool operator ==(UDecN4 left, UDecN4 right) => left.bits == right.bits;
        public static bool operator !=(UDecN4 left, UDecN4 right) => !(left == right);

        public override bool Equals(object obj) => obj is UDecN4 other && Equals(other);
        public bool Equals(UDecN4 other) => bits == other.bits;
        public override int GetHashCode() => HashCode.Combine(bits);

        #endregion
    }
}
