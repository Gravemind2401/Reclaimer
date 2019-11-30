using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
{
    public class TagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;
        private int Magic
        {
            get
            {
                if (cache.Header.DataTableAddress == 0)
                    return cache.Header.VirtualBaseAddress - cache.Header.MetadataAddress;
                else return cache.Header.VirtualBaseAddress - (cache.Header.DataTableAddress + cache.Header.DataTableSize);
            }
        }

        public TagAddressTranslator(CacheFile cache)
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
