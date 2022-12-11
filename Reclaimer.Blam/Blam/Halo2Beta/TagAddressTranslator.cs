using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo2Beta
{
    public class TagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.TagIndex[0].MetaPointer.Value - (cache.Header.IndexAddress + cache.Header.IndexSize);

        public TagAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
