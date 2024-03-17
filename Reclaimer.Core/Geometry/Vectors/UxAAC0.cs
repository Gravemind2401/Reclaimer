using Reclaimer.IO;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// <br/> Each axis has a precision of 10, 10, 12 bits respectively.
    /// <br/> Each axis has a possible value range from 0f to 1f.
    /// </summary>
    public record struct UxAAC0 : IVector3, IBufferableVector<UxAAC0>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(10, 10, 12);

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

        public UxAAC0(uint value)
        {
            bits = value;
        }

        public UxAAC0(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public UxAAC0(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UxAAC0 IBufferable<UxAAC0>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UxAAC0(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(UxAAC0 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator UxAAC0(Vector3 value) => new UxAAC0(value);

        public static explicit operator uint(UxAAC0 value) => value.bits;
        public static explicit operator UxAAC0(uint value) => new UxAAC0(value);

        #endregion
    }
}
