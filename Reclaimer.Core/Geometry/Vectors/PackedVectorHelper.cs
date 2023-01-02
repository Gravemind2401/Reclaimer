namespace Reclaimer.Geometry.Vectors
{
    internal class PackedVectorHelper
    {
        public static PackedVectorHelper CreateSigned(byte uniformPrecision) => new PackedVectorHelper(true, uniformPrecision);
        public static PackedVectorHelper CreateUnsigned(byte uniformPrecision) => new PackedVectorHelper(false, uniformPrecision);

        public static PackedVectorHelper CreateSigned(params byte[] precision) => new PackedVectorHelper(true, precision);
        public static PackedVectorHelper CreateUnsigned(params byte[] precision) => new PackedVectorHelper(false, precision);

        private readonly bool signed;
        private readonly BitRange[] axes;

        private PackedVectorHelper(bool signed, params byte[] precision)
        {
            this.signed = signed;
            axes = new BitRange[precision.Length];

            var offset = 0;
            for (var i = 0; i < precision.Length; i++)
            {
                axes[i] = new BitRange(offset, precision[i]);
                offset += precision[i];
            }
        }

        private float Normalise(in float value, in BitRange bitRange)
        {
            return signed
                ? Utils.Clamp(value, -1f, 1f) * bitRange.SignedScale
                : Utils.Clamp(value, 0f, 1f) * bitRange.UnsignedScale;
        }

        #region 8 or 16-bit Normalised

        public float GetValue(in byte bits) => GetValue(bits, 0);
        public void SetValue(ref byte bits, in float value) => bits = (byte)Normalise(value, axes[0]);

        public float GetValue(in ushort bits) => GetValue(bits, 0);
        public void SetValue(ref ushort bits, in float value) => bits = (ushort)Normalise(value, axes[0]);

        #endregion

        #region 32-bit Packed

        public float GetX(in uint bits) => GetValue(bits, 0);
        public void SetX(ref uint bits, float value) => SetValue(ref bits, 0, value);

        public float GetY(in uint bits) => GetValue(bits, 1);
        public void SetY(ref uint bits, float value) => SetValue(ref bits, 1, value);

        public float GetZ(in uint bits) => GetValue(bits, 2);
        public void SetZ(ref uint bits, float value) => SetValue(ref bits, 2, value);

        public float GetW(in uint bits) => GetValue(bits, 3);
        public void SetW(ref uint bits, float value) => SetValue(ref bits, 3, value);

        private float GetValue(in uint bits, int index)
        {
            return signed
                ? GetSignedBits(bits, index) / axes[index].SignedScale
                : GetUnsignedBits(bits, index) / axes[index].UnsignedScale;
        }

        private uint GetUnsignedBits(in uint bits, int axis) => (bits >> axes[axis].Offset) & axes[axis].LengthMask;
        private int GetSignedBits(in uint bits, int axis)
        {
            var signExtend = (bits & axes[axis].SignMask) > 0 ? axes[axis].SignExtend : 0;
            return unchecked((int)(GetUnsignedBits(bits, axis) | signExtend));
        }

        private void SetValue(ref uint bits, int index, float value)
        {
            ref var bitRange = ref axes[index];
            value = Normalise(value, bitRange);

            if (signed)
            {
                var valueBits = ((int)value & (int)bitRange.LengthMask) << bitRange.Offset;
                bits = bits & ~bitRange.OffsetMask | unchecked((uint)valueBits);
            }
            else
            {
                var valueBits = ((uint)value & bitRange.LengthMask) << bitRange.Offset;
                bits = bits & ~bitRange.OffsetMask | valueBits;
            }
        }

        #endregion

        private readonly record struct BitRange(int Offset, int Length)
        {
            public readonly uint LengthMask = (uint)((1UL << Length) - 1);
            public readonly uint OffsetMask = (uint)((1UL << Length) - 1) << Offset;

            public readonly uint SignMask = (uint)(1UL << (Offset + Length - 1));
            public readonly uint SignExtend = uint.MaxValue << Length;

            public readonly float UnsignedScale = (1UL << Length) - 1;
            public readonly float SignedScale = (1UL << (Length - 1)) - 1;
        }
    }
}
