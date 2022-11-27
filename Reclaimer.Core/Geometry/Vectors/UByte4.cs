using Reclaimer.IO;
using System;
using System.Numerics;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with 8-bit unsigned integer components.
    /// </summary>
    public record struct UByte4(byte X, byte Y, byte Z, byte W) : IVector4, IReadOnlyVector4, IBufferableVector<UByte4>
    {
        private const int packSize = 1;
        private const int structureSize = 4;

        private static byte Clamp(float value) => (byte)Utils.Clamp(value, 0f, byte.MaxValue);

        public UByte4(Vector4 value)
            : this(Clamp(value.X), Clamp(value.Y), Clamp(value.Z), Clamp(value.W))
        { }

        public override string ToString() => $"[{X}, {Y}, {Z}, {W}]";

        #region IReadOnlyVector4

        float IReadOnlyVector2.X => X;
        float IReadOnlyVector2.Y => Y;
        float IReadOnlyVector3.Z => Z;
        float IReadOnlyVector4.W => W;

        #endregion

        #region IVector4

        float IVector2.X
        {
            get => X;
            set => X = Clamp(value);
        }

        float IVector2.Y
        {
            get => Y;
            set => Y = Clamp(value);
        }

        float IVector3.Z
        {
            get => Z;
            set => Z = Clamp(value);
        }

        float IVector4.W
        {
            get => W;
            set => W = Clamp(value);
        }

        #endregion

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static UByte4 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UByte4(buffer[0], buffer[1], buffer[2], buffer[3]);
        void IBufferable<UByte4>.WriteToBuffer(Span<byte> buffer) => (buffer[0], buffer[1], buffer[2], buffer[3]) = (X, Y, Z, W);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(UByte4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator UByte4(Vector4 value) => new UByte4(value);

        #endregion
    }
}
