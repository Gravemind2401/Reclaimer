using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class cache_file_resource_layout_table
    {
        [Offset(12)]
        public BlockCollection<SharedCacheBlock> SharedCaches { get; set; }

        [Offset(24)]
        public BlockCollection<PageBlock> Pages { get; set; }

        [Offset(36)]
        public BlockCollection<SoundInfoBlock> SoundInfo { get; set; }

        [Offset(48)]
        public BlockCollection<SegmentBlock> Segments { get; set; }
    }

    [FixedSize(264)]
    public class SharedCacheBlock
    {
        [Offset(0)]
        [NullTerminated(Length = 32)]
        public string FileName { get; set; }

        public override string ToString() => FileName;
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
    public class SoundInfoBlock
    {
        [Offset(0)]
        public int RawSize { get; set; }

        [Offset(4)]
        public BlockCollection<SoundPermutationInfoBlock> SoundPermutationInfo { get; set; }
    }

    [FixedSize(16)]
    public class SoundPermutationInfoBlock
    {
        [Offset(4)]
        public int PermutationSize { get; set; }
    }

    [FixedSize(16)]
    public class SegmentBlock
    {
        [Offset(0)]
        public short RequiredPageIndex { get; set; }

        [Offset(2)]
        public short OptionalPageIndex { get; set; }

        [Offset(4)]
        public int RequiredPageOffset { get; set; }

        [Offset(8)]
        public int OptionalPageOffset { get; set; }

        [Offset(12)]
        public short SoundNumber { get; set; }

        [Offset(14)]
        public short SoundRawIndex { get; set; }
    }
}
