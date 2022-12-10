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
    /// Each axis has a possible value range from -1f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public record struct Int16N2 : IVector2, IBufferableVector<Int16N2>
    {
        private const int packSize = sizeof(ushort);
        private const int structureSize = sizeof(ushort) * 2;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSigned(16);

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

        public Int16N2(ushort x, ushort y)
        {
            (xbits, ybits) = (x, y);
        }

        public Int16N2(Vector2 value)
            : this(value.X, value.Y)
        { }

        public Int16N2(float x, float y)
        {
            xbits = ybits = default;
            (X, Y) = (x, y);
        }

        private Int16N2(ReadOnlySpan<ushort> values)
            : this(values[0], values[1])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static Int16N2 IBufferable<Int16N2>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new Int16N2(MemoryMarshal.Cast<byte, ushort>(buffer));
        void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<ushort, byte>(new[] { xbits, ybits }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector2(Int16N2 value) => new Vector2(value.X, value.Y);
        public static explicit operator Int16N2(Vector2 value) => new Int16N2(value);

        #endregion
    }
}
