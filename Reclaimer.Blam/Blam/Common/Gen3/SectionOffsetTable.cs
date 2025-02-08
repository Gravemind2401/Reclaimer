using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;

namespace Reclaimer.Blam.Common.Gen3
{
    public class SectionOffsetTable : IReadOnlyList<uint>, IWriteable
    {
        private readonly uint[] sectionOffsets;

        public SectionOffsetTable(ICacheFile cache, EndianReader reader)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(reader);

            if (cache.Metadata.Generation < CacheGeneration.Gen3)
                throw new ArgumentException("CacheFile must be Gen3 or later", nameof(cache));

            sectionOffsets = new uint[4];

            for (var i = 0; i < sectionOffsets.Length; i++)
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
