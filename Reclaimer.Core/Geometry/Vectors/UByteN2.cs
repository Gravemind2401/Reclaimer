using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with 8-bit normalised components.
    /// <br/> Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public record struct UByteN2 : IVector2, IBufferableVector<UByteN2>
    {
        private const int packSize = sizeof(byte);
        private const int structureSize = sizeof(byte) * 2;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(8);

        private byte xbits, ybits;

        #region Axis Properties

        public float X
        {
            readonly get => helper.GetValue(in xbits);
            set => helper.SetValue(ref xbits, value);
        }

        public float Y
        {
            readonly get => helper.GetValue(in ybits);
            set => helper.SetValue(ref ybits, value);
        }

        #endregion

        public UByteN2(byte x, byte y)
        {
            (xbits, ybits) = (x, y);
        }

        public UByteN2(Vector2 value)
            : this(value.X, value.Y)
        { }

        public UByteN2(float x, float y)
        {
            xbits = ybits = default;
            (X, Y) = (x, y);
        }

        private UByteN2(ReadOnlySpan<byte> values)
            : this(values[0], values[1])
        { }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UByteN2 IBufferable<UByteN2>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UByteN2(buffer);
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => (buffer[0], buffer[1]) = (xbits, ybits);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(UByteN2 value) => new Vector2(value.X, value.Y);
        public static explicit operator UByteN2(Vector2 value) => new UByteN2(value);

        #endregion
    }
}
