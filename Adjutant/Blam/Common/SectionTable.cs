using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    [CLSCompliant(false)]
    public class SectionTable : IReadOnlyList<SectionLayout>
    {
        private readonly SectionLayout[] sections;

        public SectionTable(ICacheFile cache, EndianReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            sections = new SectionLayout[4];

            for (int i = 0; i < sections.Length; i++)
                sections[i] = reader.ReadObject<SectionLayout>();
        }

        #region IReadOnlyList
        public SectionLayout this[int index] => sections[index];

        public int Count => sections.Length;

        public IEnumerator<SectionLayout> GetEnumerator() => ((IReadOnlyList<SectionLayout>)sections).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => sections.GetEnumerator(); 
        #endregion
    }

    [FixedSize(8)]
    [CLSCompliant(false)]
    public struct SectionLayout
    {
        [Offset(0)]
        public uint Address { get; set; }

        [Offset(4)]
        public uint Size { get; set; }
    }
}
