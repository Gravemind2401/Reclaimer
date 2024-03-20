using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo1
{
    public class BspAddressTranslator : IAddressTranslator
    {
        private readonly StructureBspBlock data;

        public int Magic => data.Magic - data.MetadataAddress;

        public int TagAddress => data.MetadataAddress;

        public BspAddressTranslator(CacheFile cache, int id)
        {
            var scnr = cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>();
            var bspData = scnr.StructureBsps.SingleOrDefault(i => i.BspReference.TagId == id);
            data = bspData ?? throw new InvalidOperationException();
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
