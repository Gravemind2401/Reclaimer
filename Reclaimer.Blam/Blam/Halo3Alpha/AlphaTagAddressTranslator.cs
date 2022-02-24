﻿using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo3Alpha
{
    public class AlphaTagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.Header.VirtualBaseAddress - cache.Header.TagDataAddress;

        public AlphaTagAddressTranslator(CacheFile cache)
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