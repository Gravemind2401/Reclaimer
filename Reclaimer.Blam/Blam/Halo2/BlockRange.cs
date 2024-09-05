using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    /// <summary>
    /// Represents a 9-bit index and 7-bit count concatenated as a 16-bit integer.
    /// </summary>
    public record struct BlockRange : IBufferable<BlockRange>
    {
        private const int packSize = sizeof(ushort);
        private const int structureSize = sizeof(ushort);

        private const byte indexBits = 9;
        private const ushort indexMask = (1 << indexBits) - 1;

        private ushort bits;

        public ushort Index
        {
            readonly get => (ushort)(bits & indexMask);
            set => bits |= (ushort)(value & indexMask);
        }

        public byte Count
        {
            readonly get => (byte)(bits >> indexBits);
            set => bits |= (ushort)((value >> 1) << indexBits);
        }

        public readonly bool IsEmpty => Count == 0;

        public BlockRange(ushort bits)
        {
            this.bits = bits;
        }

        public BlockRange(ushort index, byte count)
        {
            bits = default;
            (Index, Count) = (index, count);
        }

        public override readonly string ToString() => IsEmpty ? "[]" : $"[{Index}..{Index + Count}]";

        #region IBufferable

        static int IBufferable.PackSize => packSize;
        static int IBufferable.SizeOf => structureSize;

        static BlockRange IBufferable<BlockRange>.ReadFromBuffer(ReadOnlySpan<byte> buffer) => new BlockRange(BitConverter.ToUInt16(buffer));
        readonly void IBufferable.WriteToBuffer(Span<byte> buffer) => BitConverter.GetBytes(bits).CopyTo(buffer);

        #endregion

        #region Cast Operators

        public static explicit operator ushort(BlockRange value) => value.bits;
        public static explicit operator BlockRange(ushort value) => new BlockRange(value);

        public static implicit operator Range(BlockRange value) => new Range(value.Index, value.Index + value.Count);

        #endregion
    }
}
