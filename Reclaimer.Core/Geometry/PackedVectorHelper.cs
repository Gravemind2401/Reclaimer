using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Geometry
{
    internal class PackedVectorHelper
    {
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

        public float GetX(in uint bits) => GetValue(in bits, 0);
        public void SetX(ref uint bits, float value) => SetValue(ref bits, 0, value);

        public float GetY(in uint bits) => GetValue(in bits, 1);
        public void SetY(ref uint bits, float value) => SetValue(ref bits, 1, value);

        public float GetZ(in uint bits) => GetValue(in bits, 2);
        public void SetZ(ref uint bits, float value) => SetValue(ref bits, 2, value);

        public float GetW(in uint bits) => GetValue(in bits, 3);
        public void SetW(ref uint bits, float value) => SetValue(ref bits, 3, value);

        private uint GetUnsignedBits(in uint bits, int axis) => (bits >> axes[axis].Offset) & axes[axis].LengthMask;
        private int GetSignedBits(in uint bits, int axis)
        {
            var signExtend = (bits & axes[axis].SignMask) > 0 ? axes[axis].SignExtend : 0;
            return unchecked((int)(GetUnsignedBits(in bits, axis) | signExtend));
        }

        private float GetValue(in uint bits, int index)
        {
            return signed
                ? GetSignedBits(in bits, index) / axes[index].SignedScale
                : GetUnsignedBits(in bits, index) / axes[index].UnsignedScale;
        }

        private void SetValue(ref uint bits, int index, float value)
        {
            ref var bitRange = ref axes[index];

            if (signed)
            {
                value = Utils.Clamp(value, -1f, 1f) * bitRange.SignedScale;
                var valueBits = ((int)value & (int)bitRange.LengthMask) << bitRange.Offset;
                bits = bits & ~bitRange.OffsetMask | unchecked((uint)valueBits);
            }
            else
            {
                value = Utils.Clamp(value, 0f, 1f) * bitRange.UnsignedScale;
                var valueBits = ((uint)value & bitRange.LengthMask) << bitRange.Offset;
                bits = bits & ~bitRange.OffsetMask | valueBits;
            }
        }

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
