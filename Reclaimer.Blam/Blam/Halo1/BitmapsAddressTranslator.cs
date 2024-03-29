﻿using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.Halo1
{
    public class BitmapsAddressTranslator : IAddressTranslator
    {
        public int TagAddress { get; }

        public BitmapsAddressTranslator(CacheFile cache, IndexItem tag)
            : this(cache, tag, cache.CreateBitmapsReader())
        { }

        public BitmapsAddressTranslator(CacheFile cache, IndexItem tag, EndianReader reader)
        {
            reader.Seek(8, SeekOrigin.Begin);
            var indexAddress = reader.ReadInt32();
            var tagIndex = tag.MetaPointer.Value;

            reader.Seek(indexAddress + tagIndex * 12 + 8, SeekOrigin.Begin);
            TagAddress = reader.ReadInt32();
        }

        public long GetAddress(long pointer) => TagAddress + (int)pointer;
        public long GetPointer(long address) => address - TagAddress;
    }
}
