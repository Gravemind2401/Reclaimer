using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct RealVector4(float X, float Y, float Z, float W) : IVector4, IBufferableVector<RealVector4>
    {
        private const int packSize = 4;
        private const int structureSize = 16;

        public RealVector4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        private RealVector4(ReadOnlySpan<float> values)
            : this(values[0], values[1], values[2], values[3])
        { }

        public RealVector4 Conjugate => new RealVector4(-X, -Y, -Z, W);

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static RealVector4 IBufferable<RealVector4>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new RealVector4(MemoryMarshal.Cast<byte, float>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { X, Y, Z, W }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(RealVector4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator RealVector4(Vector4 value) => new RealVector4(value);
        public static implicit operator RealVector4((float x, float y, float z, float w) value) => new RealVector4(value.x, value.y, value.z, value.w);

        #endregion
    }
}
