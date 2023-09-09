using Reclaimer.IO;
using System.Numerics;

// This file was automatically generated via the 'PackedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// Each axis has a precision of 10, 11, 11 bits respectively.
    /// Each axis has a possible value range from -1f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("PackedVectors.tt", "")]
    public record struct DHenN3 : IVector3, IBufferableVector<DHenN3>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSigned(10, 11, 11);

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

        #endregion

        public DHenN3(uint value)
        {
            bits = value;
        }

        public DHenN3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public DHenN3(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static DHenN3 IBufferable<DHenN3>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new DHenN3(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(DHenN3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator DHenN3(Vector3 value) => new DHenN3(value);

        public static explicit operator uint(DHenN3 value) => value.bits;
        public static explicit operator DHenN3(uint value) => new DHenN3(value);

        #endregion
    }
}
