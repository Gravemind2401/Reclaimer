using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with 16-bit unsigned integer components.
    /// </summary>
    public record struct UShort2(ushort X, ushort Y) : IVector2, IBufferableVector<UShort2>
    {
        private const int packSize = 2;
        private const int structureSize = 4;

        private static ushort Clamp(in float value) => (ushort)Utils.Clamp(value, 0f, ushort.MaxValue);

        public UShort2(Vector2 value)
            : this(Clamp(value.X), Clamp(value.Y))
        { }

        private UShort2(ReadOnlySpan<ushort> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X}, {Y}]";

        #region IVector2

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

        #endregion

        #region IBufferableVector

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UShort2 IBufferable<UShort2>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UShort2(MemoryMarshal.Cast<byte, ushort>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<ushort, byte>(new[] { X, Y }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(UShort2 value) => new Vector2(value.X, value.Y);
        public static explicit operator UShort2(Vector2 value) => new UShort2(value);

        #endregion
    }
}
