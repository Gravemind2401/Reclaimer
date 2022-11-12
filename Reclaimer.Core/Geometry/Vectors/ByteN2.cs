using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with 8-bit normalised components.
    /// Each axis has a possible value range from -1f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public struct ByteN2 : IEquatable<ByteN2>, IVector2, IReadOnlyVector2, IBufferableVector<ByteN2>
    {
        private const int packSize = 1;
        private const int structureSize = 2;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSigned(8);

        private byte xbits, ybits;

        #region Axis Properties

        public float X
        {
            get => helper.GetValue(in xbits);
            set => helper.SetValue(ref xbits, value);
        }

        public float Y
        {
            get => helper.GetValue(in ybits);
            set => helper.SetValue(ref ybits, value);
        }

        #endregion

        public ByteN2(byte x, byte y)
        {
            (xbits, ybits) = (x, y);
        }

        public ByteN2(Vector2 value)
            : this(value.X, value.Y)
        { }

        public ByteN2(float x, float y)
        {
            xbits = ybits = default;
            (X, Y) = (x, y);
        }

        private ByteN2(ReadOnlySpan<byte> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;

        private static ByteN2 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new ByteN2(buffer);
        void IBufferable<ByteN2>.WriteToBuffer(Span<byte> buffer) => (buffer[0], buffer[1]) = (xbits, ybits);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(ByteN2 value) => new Vector2(value.X, value.Y);
        public static explicit operator ByteN2(Vector2 value) => new ByteN2(value);

        #endregion

        #region Equality Operators

        public static bool operator ==(ByteN2 left, ByteN2 right) => left.xbits == right.xbits && left.ybits == right.ybits;
        public static bool operator !=(ByteN2 left, ByteN2 right) => !(left == right);

        public override bool Equals(object obj) => obj is ByteN2 other && Equals(other);
        public bool Equals(ByteN2 other) => xbits == other.xbits && ybits == other.ybits;
        public override int GetHashCode() => HashCode.Combine(xbits, ybits);

        #endregion
    }
}
