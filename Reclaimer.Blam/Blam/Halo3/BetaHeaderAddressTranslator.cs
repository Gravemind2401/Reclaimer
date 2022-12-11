using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo3
{
    public class BetaHeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => 0;

        public BetaHeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
