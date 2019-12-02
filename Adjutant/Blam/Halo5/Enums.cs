using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    public enum ModuleType : int
    {
        Halo5Server = 23,
        Halo5Forge = 27
    }

    [Flags]
    public enum FileEntryFlags : byte
    {
        Compressed = 0,
        HasBlocks = 1,
        RawFile = 2
    }
}
