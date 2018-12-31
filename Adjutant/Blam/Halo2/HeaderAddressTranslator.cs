using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.Index.Magic - (cache.Header.IndexAddress + 32);

        public HeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public int GetAddress(int pointer)
        {
            return pointer - Magic;
        }

        public int GetPointer(int address)
        {
            return address + Magic;
        }
    }
}
