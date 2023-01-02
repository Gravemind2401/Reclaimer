using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'RealVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with half-precision floating-point values.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("RealVectors.tt", "")]
    public record struct HalfVector3(Half X, Half Y, Half Z) : IVector3, IBufferableVector<HalfVector3>
    {
        private const int packSize = 2;
        private const int structureSize = 6;

        public HalfVector3(Vector3 value)
            : this((Half)value.X, (Half)value.Y, (Half)value.Z)
        { }

        private HalfVector3(ReadOnlySpan<Half> values)
            : this(values[0], values[1], values[2])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static HalfVector3 IBufferable<HalfVector3>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new HalfVector3(MemoryMarshal.Cast<byte, Half>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<Half, byte>(new[] { X, Y, Z }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(HalfVector3 value) => new Vector3((float)value.X, (float)value.Y, (float)value.Z);
        public static explicit operator HalfVector3(Vector3 value) => new HalfVector3(value);
        public static implicit operator HalfVector3((Half x, Half y, Half z) value) => new HalfVector3(value.x, value.y, value.z);

        #endregion

        #region IVector3

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

        #endregion
    }
}
