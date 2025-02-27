﻿using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public class CacheFileResourceLayoutTableTag
    {
        [Offset(12)]
        public BlockCollection<SharedCacheBlock> SharedCaches { get; set; }

        [Offset(24)]
        public BlockCollection<PageBlock> Pages { get; set; }

        [Offset(48)]
        public BlockCollection<SegmentBlock> Segments { get; set; }
    }

    [FixedSize(264)]
    [DebuggerDisplay($"{{{nameof(FileName)},nq}}")]
    public class SharedCacheBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string FileName { get; set; }
    }

    [FixedSize(88)]
    public class PageBlock
    {
        [Offset(4)]
        public short CacheIndex { get; set; }

        [Offset(8)]
        public int DataOffset { get; set; }

        [Offset(12)]
        public int CompressedSize { get; set; }

        [Offset(16)]
        public int DecompressedSize { get; set; }

        [Offset(84)]
        public short DataChunkCount { get; set; }
    }

    [FixedSize(24)]
    public class SegmentBlock
    {
        [Offset(0)]
        public int PrimaryPageOffset { get; set; }

        [Offset(4)]
        public int SecondaryPageOffset { get; set; }

        [Offset(8)]
        public int TertiaryPageOffset { get; set; }

        [Offset(12)]
        public short PrimaryPageIndex { get; set; }

        [Offset(14)]
        public short SecondaryPageIndex { get; set; }

        [Offset(16)]
        public short TertiaryPageIndex { get; set; }
    }
}
