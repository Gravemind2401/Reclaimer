using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common.Gen3
{
    public class SectionOffsetTable : IReadOnlyList<uint>, IWriteable
    {
        private readonly uint[] sectionOffsets;

        public SectionOffsetTable(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache.Metadata.Generation < CacheGeneration.Gen3)
                throw new ArgumentException();

            sectionOffsets = new uint[4];

            for (int i = 0; i < sectionOffsets.Length; i++)
                sectionOffsets[i] = reader.ReadUInt32();
        }

        public void Write(EndianWriter writer, double? version)
        {
            foreach (var offset in sectionOffsets)
                writer.Write(offset);
        }

        #region IReadOnlyList
        public uint this[int index]
        {
            get => sectionOffsets[index];
            set => sectionOffsets[index] = value;
        }

        public int Count => sectionOffsets.Length;

        public IEnumerator<uint> GetEnumerator() => ((IReadOnlyList<uint>)sectionOffsets).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => sectionOffsets.GetEnumerator();
        #endregion
    }
}
