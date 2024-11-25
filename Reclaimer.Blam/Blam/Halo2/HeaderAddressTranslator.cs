using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo2
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.TagIndex.TagClassOffset.Value - (cache.Header.IndexAddress + TagIndex.HeaderSize);

        public HeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
