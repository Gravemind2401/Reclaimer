﻿using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.MccHaloReach
{
    public class PointerExpander : IPointerExpander
    {
        private readonly int magic;

        public PointerExpander(CacheFile cache)
        {
            switch (cache.BuildString)
            {
                case "Jun 24 2019 00:36:03":
                case "Jul 30 2019 14:17:16":
                    magic = 0x10000000;
                    break;
                default:
                    magic = 0x50000000;
                    break;
            }
        }

        public long Expand(int pointer) => ((long)pointer << 2) + magic;
        public int Contract(long pointer) => (int)((pointer - magic) >> 2);
    }
}
