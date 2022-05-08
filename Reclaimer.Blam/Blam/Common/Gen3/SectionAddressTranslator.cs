using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common.Gen3
{
    public class SectionAddressTranslator : IAddressTranslator
    {
        private readonly IGen3CacheFile cache;
        private readonly int sectionIndex;

        private uint Magic => cache.Header.SectionTable[sectionIndex].Address - (cache.Header.SectionTable[sectionIndex].Address + cache.Header.SectionOffsetTable[sectionIndex]);

        public uint VirtualAddress => cache.Header.SectionTable[sectionIndex].Address;

        public SectionAddressTranslator(IGen3CacheFile cache, int sectionIndex)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.sectionIndex = sectionIndex;
        }

        public long GetAddress(long pointer) => pointer - Magic;
        public long GetPointer(long address) => address + Magic;
    }
}
