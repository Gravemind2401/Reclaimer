using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct RealVector2(float X, float Y) : IVector2, IBufferableVector<RealVector2>
    {
        private const int packSize = 4;
        private const int structureSize = 8;

        public RealVector2(Vector2 value)
            : this(value.X, value.Y)
        { }

        private RealVector2(ReadOnlySpan<float> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static RealVector2 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new RealVector2(MemoryMarshal.Cast<byte, float>(buffer));
        void IBufferable<RealVector2>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { X, Y }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(RealVector2 value) => new Vector2(value.X, value.Y);
        public static explicit operator RealVector2(Vector2 value) => new RealVector2(value);

        #endregion
    }
}
