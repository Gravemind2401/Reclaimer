using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.MccHalo2X
{
    public class PointerExpander : IPointerExpander
    {
        private readonly int magic;

        public PointerExpander(CacheFile cache)
        {
            magic = 0x7AC00000;
        }

        public long Expand(int pointer) => ((long)pointer << 2) + magic;
        public int Contract(long pointer) => (int)((pointer - magic) >> 2);
    }
}
