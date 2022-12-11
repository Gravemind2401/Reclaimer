using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Halo3Alpha
{
    public class AlphaHeaderAddressTranslator : IAddressTranslator
    {
        private readonly CacheFile cache;

        private int Magic => 0;

        public AlphaHeaderAddressTranslator(CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => (int)pointer - Magic;
        public long GetPointer(long address) => (int)address + Magic;
    }
}
