using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;
        private int Magic
        {
            get
            {
                if (cache.Header.DataTableAddress == 0)
                    return 0;
                else return cache.Header.StringTableIndexPointer.Value - cache.HeaderSize;
            }
        }

        public HeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer)
        {
            return (int)pointer - Magic;
        }

        public long GetPointer(long address)
        {
            return (int)address + Magic;
        }
    }
}
