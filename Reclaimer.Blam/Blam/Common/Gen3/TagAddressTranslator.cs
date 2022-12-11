using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen3
{
    public class TagAddressTranslator : IAddressTranslator
    {
        private readonly IGen3CacheFile cache;
        private long Magic => cache.Header.VirtualBaseAddress - (cache.Header.SectionTable[2].Address + cache.Header.SectionOffsetTable[2]);

        public TagAddressTranslator(IGen3CacheFile cache)
        {
            this.cache = cache;
        }

        public long GetAddress(long pointer) => pointer - Magic;
        public long GetPointer(long address) => address + Magic;
    }
}
