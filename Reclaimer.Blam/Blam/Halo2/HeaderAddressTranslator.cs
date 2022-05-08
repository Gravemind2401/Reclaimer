using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo2
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.TagIndex.Magic - (cache.Header.IndexAddress + cache.TagIndex.HeaderSize);

        public HeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
