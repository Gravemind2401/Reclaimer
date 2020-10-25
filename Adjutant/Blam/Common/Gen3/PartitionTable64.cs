using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public class PartitionTable64 : IReadOnlyList<PartitionLayout64>
    {
        private readonly PartitionLayout64[] partitions;

        public PartitionTable64(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            partitions = new PartitionLayout64[6];

            for (int i = 0; i < partitions.Length; i++)
                partitions[i] = reader.ReadObject<PartitionLayout64>();
        }

        #region IReadOnlyList
        public PartitionLayout64 this[int index] => partitions[index];

        public int Count => partitions.Length;

        public IEnumerator<PartitionLayout64> GetEnumerator() => ((IReadOnlyList<PartitionLayout64>)partitions).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => partitions.GetEnumerator();
        #endregion
    }

    [FixedSize(16)]
    public struct PartitionLayout64
    {
        [Offset(0)]
        public ulong Address { get; set; }

        [Offset(8)]
        public ulong Size { get; set; }
    }
}
