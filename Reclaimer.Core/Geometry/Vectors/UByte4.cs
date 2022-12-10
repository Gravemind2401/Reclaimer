using Reclaimer.IO;
using System;
using System.Numerics;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with 8-bit unsigned integer components.
    /// </summary>
    public record struct UByte4(byte X, byte Y, byte Z, byte W) : IVector4, IBufferableVector<UByte4>
    {
        private const int packSize = 1;
        private const int structureSize = 4;

        private static byte Clamp(float value) => (byte)Utils.Clamp(value, 0f, byte.MaxValue);

        public UByte4(Vector4 value)
            : this(Clamp(value.X), Clamp(value.Y), Clamp(value.Z), Clamp(value.W))
        { }

        public override string ToString() => $"[{X}, {Y}, {Z}, {W}]";

        #region IVector4

        float IVector.X
        {
            get => X;
            set => X = Clamp(value);
        }

        float IVector.Y
        {
            get => Y;
            set => Y = Clamp(value);
        }

        float IVector.Z
        {
            get => Z;
            set => Z = Clamp(value);
        }

        float IVector.W
        {
            get => W;
            set => W = Clamp(value);
        }

        #endregion

        #region IBufferableVector

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UByte4 IBufferable<UByte4>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UByte4(buffer[0], buffer[1], buffer[2], buffer[3]);
        void IBufferable.WriteToBuffer(Span<byte> buffer) => (buffer[0], buffer[1], buffer[2], buffer[3]) = (X, Y, Z, W);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(UByte4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator UByte4(Vector4 value) => new UByte4(value);

        #endregion
    }
}
