using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo2Beta
{
    public class HeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.TagIndex.Magic - (cache.Header.IndexAddress + TagIndex.HeaderSize);

        public HeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
