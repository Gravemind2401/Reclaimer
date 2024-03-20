using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo1
{
    public class BSPAddressTranslator : IAddressTranslator
    {
        private readonly StructureBspBlock data;

        public int Magic => data.Magic - data.MetadataAddress;

        public int TagAddress => data.MetadataAddress;

        public BSPAddressTranslator(CacheFile cache, int id)
        {
            var bspData = cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>().StructureBSPs.SingleOrDefault(i => i.BSPReference.TagId == id);
            data = bspData ?? throw new InvalidOperationException();
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
