using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo2
{
    public class BSPAddressTranslator : IAddressTranslator
    {
        private readonly StructureBspBlock data;

        public int Magic => data.Magic - data.MetadataAddress;

        public int TagAddress => data.MetadataAddress;

        public BSPAddressTranslator(ICacheFile cache, int id)
        {
            var bspData = cache.TagIndex.GetGlobalTag("scnr").ReadMetadata<scenario>().StructureBsps.SingleOrDefault(i => i.BspReference.TagId == id);
            data = bspData ?? throw new InvalidOperationException();
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
