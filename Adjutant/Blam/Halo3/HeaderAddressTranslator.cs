using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic
        {
            get
            {
                if (cache.CacheType == CacheType.Halo3Beta)
                    return 0;

                return cache.Header.StringTableIndexPointer.Value - 12288; //size of header
            }
        }

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
