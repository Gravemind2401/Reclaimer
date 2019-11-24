using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class BitmapsAddressTranslator : IAddressTranslator
    {
        public int TagAddress { get; }

        public BitmapsAddressTranslator(CacheFile cache, IndexItem tag)
            : this(cache, tag, cache.CreateBitmapsReader())
        {

        }

        public BitmapsAddressTranslator(CacheFile cache, IndexItem tag, EndianReader reader)
        {
            reader.Seek(8, SeekOrigin.Begin);
            var indexAddress = reader.ReadInt32();
            var tagIndex = tag.MetaPointer.Value;

            reader.Seek(indexAddress + tagIndex * 12 + 8, SeekOrigin.Begin);
            TagAddress = reader.ReadInt32();
        }

        public int GetAddress(int pointer)
        {
            return TagAddress + pointer;
        }
    }
}
