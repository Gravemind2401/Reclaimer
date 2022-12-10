using Reclaimer.IO;
using System;
using System.Numerics;

// This file was automatically generated via the 'PackedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// Each axis has a precision of 10, 11, 11 bits respectively.
    /// Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("PackedVectors.tt", "")]
    public record struct UDHenN3 : IVector3, IReadOnlyVector3, IBufferableVector<UDHenN3>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(10, 11, 11);

        private uint bits;

        #region Axis Properties

        public float X
        {
            get => helper.GetX(in bits);
            set => helper.SetX(ref bits, value);
        }

        public float Y
        {
            get => helper.GetY(in bits);
            set => helper.SetY(ref bits, value);
        }

        public float Z
        {
            get => helper.GetZ(in bits);
            set => helper.SetZ(ref bits, value);
        }

        #endregion

        public UDHenN3(uint value)
        {
            bits = value;
        }

        public UDHenN3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public UDHenN3(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferableVector

        private static int PackSize => packSize;
        private static int SizeOf => structureSize;
        private static UDHenN3 ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UDHenN3(BitConverter.ToUInt32(buffer));
        void IBufferable<UDHenN3>.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(UDHenN3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator UDHenN3(Vector3 value) => new UDHenN3(value);

        public static explicit operator uint(UDHenN3 value) => value.bits;
        public static explicit operator UDHenN3(uint value) => new UDHenN3(value);

        #endregion
    }
}
