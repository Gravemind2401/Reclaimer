using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.MccHaloReach
{
    public class PointerExpander : IPointerExpander
    {
        private readonly int magic;

        public PointerExpander(CacheFile cache)
        {
            magic = cache.BuildString switch
            {
                "Jun 24 2019 00:36:03" or "Jul 30 2019 14:17:16" => 0x10000000,
                _ => 0x50000000,
            };
        }

        public long Expand(int pointer) => ((long)pointer << 2) + magic;
        public int Contract(long pointer) => (int)((pointer - magic) >> 2);
    }
}
