using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public enum SampleRate : byte
    {
        x22050Hz = 0,
        x44100Hz = 1,
        x32000Hz = 2
    }

    public enum Encoding : byte
    {
        Mono = 0,
        Stereo = 1,
        Surround = 2,
        Surround5_1 = 3
    }

    public class sound_cache_file_gestalt
    {
        [Offset(0)]
        public BlockCollection<Codec> Codecs { get; set; }

        [Offset(36)]
        public BlockCollection<SoundName> SoundNames { get; set; }

        [Offset(60, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(72, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<PitchRange> PitchRanges { get; set; }

        [Offset(72, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(84, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<SoundPermutation> SoundPermutations { get; set; }

        [Offset(148, MaxVersion = (int)CacheType.Halo3ODST)]
        [Offset(160, MinVersion = (int)CacheType.Halo3ODST)]
        public BlockCollection<SoundPermutationChunk> SoundPermutationChunk { get; set; }
    }

    [FixedSize(3)]
    public class Codec
    {
        [Offset(0)]
        public SampleRate SampleRate { get; set; }

        [Offset(1)]
        public Encoding Encoding { get; set; }

        [Offset(2)]
        public byte CompressionCodec { get; set; }

        public int SampleRateInt
        {
            get
            {
                return SampleRate switch
                {
                    SampleRate.x22050Hz => 22050,
                    SampleRate.x32000Hz => 32000,
                    SampleRate.x44100Hz => 44100,
                    _ => throw new NotSupportedException("Sample Rate not supported")
                };
            }
        }

        public byte[] ChannelCounts
        {
            get
            {
                return Encoding switch
                {
                    Encoding.Mono => new byte[] { 1 },
                    Encoding.Stereo => new byte[] { 2 },
                    Encoding.Surround => new byte[] { 2, 2 },
                    Encoding.Surround5_1 => new byte[] { 2, 2, 2 },
                    _ => throw new NotSupportedException("Encoding not supported")
                };
            }
        }
    }

    [FixedSize(4)]
    public class SoundName
    {
        [Offset(0)]
        public StringId Name { get; set; }
    }

    [FixedSize(12)]
    public class PitchRange
    {
        [Offset(0)]
        public ushort NameIndex { get; set; }

        [Offset(2)]
        public short ParametersIndex { get; set; }

        [Offset(4)]
        public short EncodedPermDataIndex { get; set; }

        [Offset(6)]
        public short EncodedRuntimePermFlagIndex { get; set; }

        [Offset(8)]
        public short EncodedPermutationCount { get; set; }

        [Offset(10)]
        public ushort FirstPermutationIndex { get; set; }

        public int PermutationCount => (EncodedPermutationCount >> 4) & 63;
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
        public int BlockIndex { get; set; }

        [Offset(12)]
        public short BlockCount { get; set; }

        [Offset(14)]
        public short EncodedPermIndex { get; set; }
    }

    [FixedSize(20)]
    public class SoundPermutationChunk
    {
        [Offset(0)]
        public int FileOffset { get; set; }

        [Offset(4)]
        public byte Flags { get; set; }

        //byte here that belongs to size (24bit)

        [Offset(6)]
        public ushort Size { get; set; }

        [Offset(8)]
        public int RuntimeIndex { get; set; }

        [Offset(12)]
        public int Unknown0 { get; set; }

        [Offset(16)]
        public int Unknown1 { get; set; }
    }
}
