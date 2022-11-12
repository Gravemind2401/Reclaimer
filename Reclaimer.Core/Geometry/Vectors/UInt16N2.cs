using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 2-dimensional vector with 16-bit normalised components.
    /// Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public struct UInt16N2 : IEquatable<UInt16N2>, IVector2, IReadOnlyVector2, IBufferableVector<UInt16N2>
    {
        private const int packSize = 2;
        private const int structureSize = 4;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(16);

        private ushort xbits, ybits;

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

        public UInt16N2(ushort x, ushort y)
        {
            (xbits, ybits) = (x, y);
        }

        public UInt16N2(Vector2 value)
            : this(value.X, value.Y)
        { }

        public UInt16N2(float x, float y)
        {
            xbits = ybits = default;
            (X, Y) = (x, y);
        }

        private UInt16N2(ReadOnlySpan<ushort> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;

        private static UInt16N2 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UInt16N2(MemoryMarshal.Cast<byte, ushort>(buffer));
        void IBufferable<UInt16N2>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<ushort, byte>(new[] { xbits, ybits }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(UInt16N2 value) => new Vector2(value.X, value.Y);
        public static explicit operator UInt16N2(Vector2 value) => new UInt16N2(value);

        #endregion

        #region Equality Operators

        public static bool operator ==(UInt16N2 left, UInt16N2 right) => left.xbits == right.xbits && left.ybits == right.ybits;
        public static bool operator !=(UInt16N2 left, UInt16N2 right) => !(left == right);

        public override bool Equals(object obj) => obj is UInt16N2 other && Equals(other);
        public bool Equals(UInt16N2 other) => xbits == other.xbits && ybits == other.ybits;
        public override int GetHashCode() => HashCode.Combine(xbits, ybits);

        #endregion
    }
}
