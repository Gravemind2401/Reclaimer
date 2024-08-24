using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public class cache_file_resource_layout_table
    {
        [Offset(12)]
        public BlockCollection<SharedCacheBlock> SharedCaches { get; set; }

        [Offset(24)]
        public BlockCollection<PageBlock> Pages { get; set; }

        [Offset(36)]
        public BlockCollection<SizeGroupBlock> SizeGroups { get; set; }

        [Offset(48, MaxVersion = (int)CacheType.MccHalo3)]
        [Offset(60, MinVersion = (int)CacheType.MccHalo3, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(48, MinVersion = (int)CacheType.Halo3ODST, MaxVersion = (int)CacheType.MccHalo3ODST)]
        [Offset(60, MinVersion = (int)CacheType.MccHalo3ODST)]
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

    [FixedSize(16)]
    public class SizeGroupBlock
    {
        [Offset(0)]
        public int TotalSize { get; set; }

        [Offset(4)]
        public BlockCollection<SizeBlock> Sizes { get; set; }
    }

    [FixedSize(8)]
    public class SizeBlock
    {
        [Offset(0)]
        public int Offset { get; set; }

        [Offset(4)]
        public int DataSize { get; set; }
    }

    [FixedSize(16)]
    public class SegmentBlock
    {
        [Offset(0)]
        public short PrimaryPageIndex { get; set; }

        [Offset(2)]
        public short SecondaryPageIndex { get; set; }

        [Offset(4)]
        public int PrimaryPageOffset { get; set; }

        [Offset(8)]
        public int SecondaryPageOffset { get; set; }

        [Offset(12)]
        public short PrimarySizeIndex { get; set; }

        [Offset(14)]
        public short SecondarySizeIndex { get; set; }
    }
}
