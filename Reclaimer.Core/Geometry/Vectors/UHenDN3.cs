using Reclaimer.IO;

// This file was automatically generated via the 'PackedVectors.tt' T4 template.
// Do not modify this file directly - any changes will be lost when the code is regenerated.

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// <br/> Each axis has a precision of 11, 11, 10 bits respectively.
    /// <br/> Each axis has a possible value range from 0f to 1f.
    /// </summary>
    [System.CodeDom.Compiler.GeneratedCode("PackedVectors.tt", "")]
    public record struct UHenDN3 : IVector3, IBufferableVector<UHenDN3>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(11, 11, 10);

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

        public UHenDN3(uint value)
        {
            bits = value;
        }

        public UHenDN3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public UHenDN3(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UHenDN3 IBufferable<UHenDN3>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UHenDN3(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(UHenDN3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator UHenDN3(Vector3 value) => new UHenDN3(value);

        public static explicit operator uint(UHenDN3 value) => value.bits;
        public static explicit operator UHenDN3(uint value) => new UHenDN3(value);

        #endregion
    }
}
