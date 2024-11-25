using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo2
{
    public class TagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic
        {
            get
            {
                return cache.Metadata.IsMcc
                    ? -cache.Header.IndexAddress
                    : cache.TagIndex[0].MetaPointer.Value - (cache.Header.IndexAddress + cache.Header.MetadataOffset);
            }
        }

        public TagAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
