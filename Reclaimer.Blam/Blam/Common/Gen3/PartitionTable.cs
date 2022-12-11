using Reclaimer.IO;
using System.Collections;

namespace Reclaimer.Blam.Common.Gen3
{
    public class PartitionTable : IPartitionTable
    {
        private readonly IPartitionLayout[] partitions;

        public PartitionTable(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            partitions = new IPartitionLayout[6];

            for (var i = 0; i < partitions.Length; i++)
                partitions[i] = reader.ReadObject<PartitionLayout>();
        }

        #region IPartitionTable
        public IPartitionLayout this[int index] => partitions[index];

        public int Count => partitions.Length;

        public IEnumerator<IPartitionLayout> GetEnumerator() => ((IReadOnlyList<IPartitionLayout>)partitions).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => partitions.GetEnumerator();
        #endregion
    }

    [FixedSize(8)]
    public class PartitionLayout : IPartitionLayout
    {
        [Offset(0)]
        public uint Address { get; set; }

        [Offset(4)]
        public uint Size { get; set; }

        #region IPartitionLayout
        ulong IPartitionLayout.Address
        {
            get => Address;
            set => Address = (uint)value;
        }

        ulong IPartitionLayout.Size
        {
            get => Size;
            set => Size = (uint)value;
        }
        #endregion
    }
}
