using Reclaimer.IO;
using System.Numerics;
using System.Runtime.InteropServices;

// This file was automatically generated via the 'NormalisedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector with 16-bit normalised components.
    /// <br/> Each axis has a possible value range from -1f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("NormalisedVectors.tt", "")]
    public record struct Int16N3 : IVector3, IBufferableVector<Int16N3>
    {
        private const int packSize = sizeof(ushort);
        private const int structureSize = sizeof(ushort) * 3;

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSignExtended(16);

        private ushort xbits, ybits, zbits;

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

        #endregion

        public Int16N3(ushort x, ushort y, ushort z)
        {
            (xbits, ybits, zbits) = (x, y, z);
        }

        public Int16N3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public Int16N3(float x, float y, float z)
        {
            xbits = ybits = zbits = default;
            (X, Y, Z) = (x, y, z);
        }

        private Int16N3(ReadOnlySpan<ushort> values)
            : this(values[0], values[1], values[2])
        { }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static Int16N3 IBufferable<Int16N3>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new Int16N3(MemoryMarshal.Cast<byte, ushort>(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => MemoryMarshal.Cast<ushort, byte>(new[] { xbits, ybits, zbits }).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(Int16N3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator Int16N3(Vector3 value) => new Int16N3(value);

        #endregion
    }
}
