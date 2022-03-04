using Reclaimer.Blam.Common;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo2
{
    public class sound_cache_file_gestalt
    {
        [Offset(16)]
        public BlockCollection<SoundName> SoundNames { get; set; }

        [Offset(32)]
        public BlockCollection<PitchRange> PitchRanges { get; set; }

        [Offset(40)]
        public BlockCollection<SoundPermutation> SoundPermutations { get; set; }

        [Offset(64)]
        public BlockCollection<SoundPermutationChunk> SoundPermutationChunks { get; set; }
    }

    [FixedSize(4)]
    public class SoundName
    {
        [Offset(0)]
        public StringId Name { get; set; }

        public override string ToString() => Name;
    }

    [FixedSize(12)]
    public class PitchRange
    {
        [Offset(0)]
        public short NameIndex { get; set; }

        [Offset(2)]
        public short ParametersIndex { get; set; }

        [Offset(4)]
        public short EncodedPermutationDataIndex { get; set; }

        [Offset(6)]
        public short EncodedRuntimePermFlagIndex { get; set; }

        [Offset(8)]
        public short FirstPermutationIndex { get; set; }

        [Offset(10)]
        public short PermutationCount { get; set; }
    }

    [FixedSize(16)]
    public class SoundPermutation
    {
        [Offset(0)]
        public short NameIndex { get; set; }

        [Offset(2)]
        public short EncodedSkipFraction { get; set; }

        [Offset(4)]
        public byte EncodedGain { get; set; }

        [Offset(5)]
        public byte PermInfoIndex { get; set; }

        [Offset(6)]
        public short LanguageNeutralTime { get; set; }

        [Offset(8)]
        public int SampleSize { get; set; }

        [Offset(12)]
        public short BlockIndex { get; set; }

        [Offset(14)]
        public short BlockCount { get; set; }
    }

    [FixedSize(12)]
    public class SoundPermutationChunk
    {
        [Offset(0)]
        public DataPointer DataPointer { get; set; }

        private int dataSize; //maybe 24-bit, maybe 29 with 3 flags, maybe even just 16-bit

        [Offset(4)]
        public int DataSize
        {
            get { return dataSize; }
            set { dataSize = value & 0x00FFFFFF; }
        }
    }
}
