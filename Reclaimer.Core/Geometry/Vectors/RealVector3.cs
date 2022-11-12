using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with single-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct RealVector3(float X, float Y, float Z) : IVector3, IReadOnlyVector3, IBufferableVector<RealVector3>
    {
        private const int packSize = 4;
        private const int structureSize = 12;

        public RealVector3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        private RealVector3(ReadOnlySpan<float> values)
            : this(values[0], values[1], values[2])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static RealVector3 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new RealVector3(MemoryMarshal.Cast<byte, float>(buffer));
        void IBufferable<RealVector3>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { X, Y, Z }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(RealVector3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator RealVector3(Vector3 value) => new RealVector3(value);

        #endregion
    }
}
