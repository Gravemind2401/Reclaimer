using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public class SectionAddressTranslator : IAddressTranslator
    {
        private readonly IGen3CacheFile cache;
        private readonly int sectionIndex;

        private uint Magic => cache.SectionTable[sectionIndex].Address - (cache.SectionTable[sectionIndex].Address + cache.SectionOffsetTable[sectionIndex]);

        [CLSCompliant(false)]
        public SectionAddressTranslator(IGen3CacheFile cache, int sectionIndex)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            this.sectionIndex = sectionIndex;
        }

        public long GetAddress(long pointer)
        {
            return pointer - Magic;
        }

        public long GetPointer(long address)
        {
            return address + Magic;
        }
    }
}
