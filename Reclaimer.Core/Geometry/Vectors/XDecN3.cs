using Reclaimer.IO;
using System.Numerics;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// Each axis has a precision of 10, 10, 10 bits respectively.
    /// Each axis has a possible value range from -1f to 1f.
    /// </summary>
    public record struct XDecN3 : IVector3, IBufferableVector<XDecN3>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSignShifted(10, 10, 10);

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

        public XDecN3(uint value)
        {
            bits = value;
        }

        public XDecN3(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public XDecN3(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static XDecN3 IBufferable<XDecN3>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new XDecN3(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(XDecN3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator XDecN3(Vector3 value) => new XDecN3(value);

        public static explicit operator uint(XDecN3 value) => value.bits;
        public static explicit operator XDecN3(uint value) => new XDecN3(value);

        #endregion
    }
}
