using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo3
{
    public class BetaTagAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => cache.Header.VirtualBaseAddress - cache.Header.TagDataAddress;

        public BetaTagAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
