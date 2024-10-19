using Reclaimer.IO;
using System.Runtime.InteropServices;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 1-dimensional vector with single-precision floating-point values.
    /// </summary>
    public record struct RealVector1(float X) : IVector1, IBufferableVector<RealVector1>
    {
        private const int packSize = 4;
        private const int structureSize = 4;

        private RealVector1(ReadOnlySpan<float> values)
            : this(values[0])
        { }

        public override readonly string ToString() => $"[{X:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static RealVector1 IBufferable<RealVector1>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new RealVector1(MemoryMarshal.Cast<byte, float>(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<float, byte>(new[] { X }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator float(RealVector1 value) => value.X;
        public static explicit operator RealVector1(float value) => new RealVector1(value);

        #endregion
    }
}
