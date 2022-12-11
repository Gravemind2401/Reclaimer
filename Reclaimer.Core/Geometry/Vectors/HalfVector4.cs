using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct HalfVector4(Half X, Half Y, Half Z, Half W) : IVector4, IBufferableVector<HalfVector4>
    {
        private const int packSize = 2;
        private const int structureSize = 8;

        public HalfVector4(Vector4 value)
            : this((Half)value.X, (Half)value.Y, (Half)value.Z, (Half)value.W)
        { }

        private HalfVector4(ReadOnlySpan<Half> values)
            : this(values[0], values[1], values[2], values[3])
        { }

        public HalfVector4 Conjugate => new HalfVector4(-X, -Y, -Z, W);

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static HalfVector4 IBufferable<HalfVector4>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new HalfVector4(MemoryMarshal.Cast<byte, Half>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<Half, byte>(new[] { X, Y, Z, W }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(HalfVector4 value) => new Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);
        public static explicit operator HalfVector4(Vector4 value) => new HalfVector4(value);
        public static implicit operator HalfVector4((Half x, Half y, Half z, Half w) value) => new HalfVector4(value.x, value.y, value.z, value.w);

        #endregion

        #region IVector4

        float IVector.X
        {
            get => (float)X;
            set => X = (Half)value;
        }

        float IVector.Y
        {
            get => (float)Y;
            set => Y = (Half)value;
        }

        float IVector.Z
        {
            get => (float)Z;
            set => Z = (Half)value;
        }

        float IVector.W
        {
            get => (float)W;
            set => W = (Half)value;
        }

        #endregion
    }
}
