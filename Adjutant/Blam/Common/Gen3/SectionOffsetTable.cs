using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public class SectionOffsetTable : IReadOnlyList<uint>
    {
        private readonly uint[] sectionOffsets;

        public SectionOffsetTable(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache.CacheType.GetCacheGeneration() < 3)
                throw new ArgumentException();

            sectionOffsets = new uint[4];

            for (int i = 0; i < sectionOffsets.Length; i++)
                sectionOffsets[i] = reader.ReadUInt32();
        }

        #region IReadOnlyList
        public uint this[int index] => sectionOffsets[index];

        public int Count => sectionOffsets.Length;

        public IEnumerator<uint> GetEnumerator() => ((IReadOnlyList<uint>)sectionOffsets).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => sectionOffsets.GetEnumerator(); 
        #endregion
    }
}
