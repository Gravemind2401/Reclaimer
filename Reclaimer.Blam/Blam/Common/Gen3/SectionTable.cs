using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;

namespace Reclaimer.Blam.Common.Gen3
{
    public class SectionTable : IReadOnlyList<SectionLayout>, IWriteable
    {
        private readonly SectionLayout[] sections;

        public SectionTable(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            sections = new SectionLayout[4];

            for (var i = 0; i < sections.Length; i++)
                sections[i] = reader.ReadObject<SectionLayout>();
        }

        public void Write(EndianWriter writer, double? version)
        {
            foreach (var section in sections)
                writer.WriteObject(section);
        }

        #region IReadOnlyList
        public SectionLayout this[int index] => sections[index];

        public int Count => sections.Length;

        public IEnumerator<SectionLayout> GetEnumerator() => ((IReadOnlyList<SectionLayout>)sections).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => sections.GetEnumerator();
        #endregion
    }

    [FixedSize(8)]
    public class SectionLayout
    {
        [Offset(0)]
        public uint Address { get; set; }

        [Offset(4)]
        public uint Size { get; set; }
    }
}
