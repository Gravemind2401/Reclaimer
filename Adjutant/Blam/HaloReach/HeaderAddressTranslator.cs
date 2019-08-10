using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.HaloReach
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;
        private int Magic => cache.Header.StringTableIndexPointer.Value - cache.HeaderSize;

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
