using Reclaimer.IO;
using System.Numerics;

// This file was automatically generated via the 'PackedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector packed into 32 bits.
    /// <br/> Each axis has a precision of 10, 10, 10, 2 bits respectively.
    /// <br/> Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("PackedVectors.tt", "")]
    public record struct UDecN4 : IVector4, IBufferableVector<UDecN4>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(10, 10, 10, 2);

        private uint bits;

        #region Axis Properties

        public float X
        {
            readonly get => helper.GetX(in bits);
            set => helper.SetX(ref bits, value);
        }

        public float Y
        {
            readonly get => helper.GetY(in bits);
            set => helper.SetY(ref bits, value);
        }

        public float Z
        {
            readonly get => helper.GetZ(in bits);
            set => helper.SetZ(ref bits, value);
        }

        public float W
        {
            readonly get => helper.GetW(in bits);
            set => helper.SetW(ref bits, value);
        }

        #endregion

        public UDecN4(uint value)
        {
            bits = value;
        }

        public UDecN4(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        public UDecN4(float x, float y, float z, float w)
        {
            bits = default;
            (X, Y, Z, W) = (x, y, z, w);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UDecN4 IBufferable<UDecN4>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UDecN4(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(UDecN4 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator UDecN4(Vector4 value) => new UDecN4(value);

        public static explicit operator uint(UDecN4 value) => value.bits;
        public static explicit operator UDecN4(uint value) => new UDecN4(value);

        #endregion
    }
}
