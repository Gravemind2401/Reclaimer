using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class BSPAddressTranslator : IAddressTranslator
    {
        private readonly StructureBSP data;

        private int Magic => data.Magic - data.MetadataAddress;

        public int TagAddress => data.MetadataAddress;

        public BSPAddressTranslator(CacheFile cache, int id)
        {
            var scnr = cache.Index
                .Single(i => i.ClassCode == "scnr")
                .ReadMetadata<scenario>();

            var bspData = scnr.StructureBSPs.SingleOrDefault(i => (i.BSPReference.TagId) == id);
            if (bspData == null)
                throw new InvalidOperationException();

            data = bspData;
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
