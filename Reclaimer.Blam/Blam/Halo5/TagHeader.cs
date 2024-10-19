using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    [FixedSize(80)]
    public class TagHeader
    {
        [Offset(0)]
        public int Header { get; set; }

        [Offset(4)]
        public ModuleType Version { get; set; }

        [Offset(16)]
        public long AssetChecksum { get; set; }

        [Offset(28)]
        public int DependencyCount { get; set; }

        [Offset(32)]
        public int DataBlockCount { get; set; }

        [Offset(36)]
        public int TagStructureCount { get; set; }

        [Offset(40)]
        public int DataReferenceCount { get; set; }

        [Offset(44)]
        public int TagReferenceCount { get; set; }

        [Offset(48)]
        public int StringIdCount { get; set; }

        [Offset(52)]
        public int StringTableSize { get; set; }

        [Offset(56)]
        public int ZonesetDataSize { get; set; }

        [Offset(60)]
        public int HeaderSize { get; set; }

        [Offset(64)]
        public int DataSize { get; set; }

        [Offset(68)]
        public int ResourceDataSize { get; set; }
    }
}
