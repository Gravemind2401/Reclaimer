using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    [FixedSize(80)]
    public class TagHeader
    {
        [Offset(0)]
        public int Header { get; set; }

        //note that infinite still says 27 (H5F) for this, overriding as HaloInfinite for now
        public ModuleType Version { get; set; } = ModuleType.HaloInfinite;

        [Offset(8)]
        public long AssetHash { get; set; }

        [Offset(16)]
        public long AssetChecksum { get; set; }

        [Offset(24)]
        public int DependencyCount { get; set; }

        [Offset(28)]
        public int DataBlockCount { get; set; }

        [Offset(32)]
        public int TagStructureCount { get; set; }

        [Offset(36)]
        public int DataReferenceCount { get; set; }

        [Offset(40)]
        public int TagReferenceCount { get; set; }

        [Offset(44)]
        public int StringTableSize { get; set; }

        [Offset(48)]
        public int ZonesetDataSize { get; set; }

        [Offset(56)]
        public int HeaderSize { get; set; }

        [Offset(60)]
        public int DataSize { get; set; }

        [Offset(64)]
        public int ResourceDataSize { get; set; }
    }
}
