using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with 8-bit normalised components.
    /// Each axis has a possible value range from -1f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public record struct ByteN4 : IVector4, IBufferableVector<ByteN4>
    {
        private const int packSize = sizeof(byte);
        private const int structureSize = sizeof(byte) * 4;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSigned(8);

        private byte xbits, ybits, zbits, wbits;

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

        public float Z
        {
            readonly get => helper.GetValue(in zbits);
            set => helper.SetValue(ref zbits, value);
        }

        public float W
        {
            readonly get => helper.GetValue(in wbits);
            set => helper.SetValue(ref wbits, value);
        }

        #endregion

        public ByteN4(byte x, byte y, byte z, byte w)
        {
            (xbits, ybits, zbits, wbits) = (x, y, z, w);
        }

        public ByteN4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        public ByteN4(float x, float y, float z, float w)
        {
            xbits = ybits = zbits = wbits = default;
            (X, Y, Z, W) = (x, y, z, w);
        }

        private ByteN4(ReadOnlySpan<byte> values)
            : this(values[0], values[1], values[2], values[3])
        { }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static ByteN4 IBufferable<ByteN4>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new ByteN4(buffer);
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => (buffer[0], buffer[1], buffer[2], buffer[3]) = (xbits, ybits, zbits, wbits);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(ByteN4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator ByteN4(Vector4 value) => new ByteN4(value);

        #endregion
    }
}
