﻿using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    public class scenario
    {
        [Offset(1444)]
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
    }

    [FixedSize(32)]
    [DebuggerDisplay($"{{{nameof(BspReference)},nq}}")]
    public class StructureBspBlock
    {
        [Offset(0)]
        public int MetadataAddress { get; set; }

        [Offset(4)]
        public int Size { get; set; }

        [Offset(8)]
        public int Magic { get; set; }

        [Offset(16)]
        public TagReference BspReference { get; set; }
    }
}
