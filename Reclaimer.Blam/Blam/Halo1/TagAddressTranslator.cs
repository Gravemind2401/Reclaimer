using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo1
{
    public class TagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.TagIndex.Magic - (cache.Header.IndexAddress + cache.TagIndex.HeaderSize);

        public TagAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
