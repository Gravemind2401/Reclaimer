using Reclaimer.IO;

namespace Reclaimer.Geometry.Vectors
{
    /// <summary>
    /// A 4-dimensional vector packed into 32 bits.
    /// <br/> Each axis has a precision of 10, 10, 10, 2 bits respectively.
    /// <br/> Each axis has a possible value range from -1f to 1f.
    /// </summary>
    public record struct NxAAA2 : IVector4, IBufferableVector<NxAAA2>
    {
        private const int packSize = sizeof(uint);
        private const int structureSize = sizeof(uint);

        private static readonly PackedVectorHelper helper = PackedVectorHelper.CreateSignShifted(10, 10, 10, 2);

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

        public float W
        {
            readonly get => helper.GetW(bits);
            set => helper.SetW(ref bits, value);
        }

        #endregion

        public NxAAA2(uint value)
        {
            bits = value;
        }

        public NxAAA2(Vector4 value)
            : this(value.X, value.Y, value.Z, value.W)
        { }

        public NxAAA2(float x, float y, float z, float w)
        {
            bits = default;
            (X, Y, Z, W) = (x, y, z, w);
        }

        public override readonly string ToString() => $"[{X:F6}, {Y:F6}, {Z:F6}, {W:F6}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;
        static NxAAA2 IBufferable<NxAAA2>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new NxAAA2(BitConverter.ToUInt32(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator Vector4(NxAAA2 value) => new Vector4(value.X, value.Y, value.Z, value.W);
        public static explicit operator NxAAA2(Vector4 value) => new NxAAA2(value);

        public static explicit operator uint(NxAAA2 value) => value.bits;
        public static explicit operator NxAAA2(uint value) => new NxAAA2(value);

        #endregion
    }
}
