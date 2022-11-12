using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct HalfVector2(Half X, Half Y) : IVector2, IReadOnlyVector2, IBufferableVector<HalfVector2>
    {
        private const int packSize = 2;
        private const int structureSize = 4;

        public HalfVector2(Vector2 value)
            : this((Half)value.X, (Half)value.Y)
        { }

        private HalfVector2(ReadOnlySpan<Half> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static HalfVector2 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new HalfVector2(MemoryMarshal.Cast<byte, Half>(buffer));
        void IBufferable<HalfVector2>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<Half, byte>(new[] { X, Y }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(HalfVector2 value) => new Vector2((float)value.X, (float)value.Y);
        public static explicit operator HalfVector2(Vector2 value) => new HalfVector2(value);

        #endregion

        #region IVector2

        float IVector2.X
        {
            get => (float)X;
            set => X = (Half)value;
        }
        float IVector2.Y
        {
            get => (float)Y;
            set => Y = (Half)value;
        }

        #endregion

        #region IReadOnlyVector2

        float IReadOnlyVector2.X => (float)X;
        float IReadOnlyVector2.Y => (float)Y;

        #endregion
    }
}
