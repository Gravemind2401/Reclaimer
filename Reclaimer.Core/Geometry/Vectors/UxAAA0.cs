using Reclaimer.IO;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 3-dimensional vector packed into 32 bits.
    /// <br/> Each axis has a precision of 10, 10, 10 bits respectively.
    /// <br/> Each axis has a possible value range from 0f to 1f.
    /// </summary>
    public record struct UxAAA0 : IVector3, IBufferableVector<UxAAA0>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateUnsigned(10, 10, 10);

        private uint bits;

        #region Axis Properties

        public float X
        {
            readonly get => helper.GetX(bits);
            set => helper.SetX(ref bits, value);
        }

        public float Y
        {
            readonly get => helper.GetY(bits);
            set => helper.SetY(ref bits, value);
        }

        public float Z
        {
            readonly get => helper.GetZ(bits);
            set => helper.SetZ(ref bits, value);
        }

        #endregion

        public UxAAA0(uint value)
        {
            bits = value;
        }

        public UxAAA0(Vector3 value)
            : this(value.X, value.Y, value.Z)
        { }

        public UxAAA0(float x, float y, float z)
        {
            bits = default;
            (X, Y, Z) = (x, y, z);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static UxAAA0 IBufferable<UxAAA0>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new UxAAA0(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector3(UxAAA0 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator UxAAA0(Vector3 value) => new UxAAA0(value);

        public static explicit operator uint(UxAAA0 value) => value.bits;
        public static explicit operator UxAAA0(uint value) => new UxAAA0(value);

        #endregion
    }
}
