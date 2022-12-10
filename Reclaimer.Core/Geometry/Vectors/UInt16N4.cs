using Reclaimer.IO;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector with 16-bit normalised components.
    /// Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public record struct UInt16N4 : IVector4, IReadOnlyVector4, IBufferableVector<UInt16N4>
    {
        private const int packSize = 2;
        private const int structureSize = 8;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(16);

        private ushort xbits, ybits, zbits, wbits;

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

        public float Z
        {
            get => helper.GetValue(in zbits);
            set => helper.SetValue(ref zbits, value);
        }

        public float W
        {
            get => helper.GetValue(in wbits);
            set => helper.SetValue(ref wbits, value);
        }

        #endregion

        public UInt16N4(ushort x, ushort y, ushort z, ushort w)
        {
            (xbits, ybits, zbits, wbits) = (x, y, z, w);
        }

        public UInt16N4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        public UInt16N4(float x, float y, float z, float w)
        {
            xbits = ybits = zbits = wbits = default;
            (X, Y, Z, W) = (x, y, z, w);
        }

        private UInt16N4(ReadOnlySpan<ushort> values)
            : this(values[0], values[1], values[2], values[3])
        { }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;

        private static UInt16N4 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UInt16N4(MemoryMarshal.Cast<byte, ushort>(buffer));
        void IBufferable<UInt16N4>.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<ushort, byte>(new[] { xbits, ybits, zbits, wbits }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(UInt16N4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator UInt16N4(Vector4 value) => new UInt16N4(value);

        #endregion
    }
}
