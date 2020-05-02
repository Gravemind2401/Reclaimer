using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.HaloReach
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

        [Offset(72)]
        public BlockCollection<Playback> Playbacks { get; set; }

        [Offset(84)]
        public BlockCollection<SoundPermutation> SoundPermutations { get; set; }

        [Offset(172)]
        public BlockCollection<DataBlock> DataBlocks { get; set; }
    }

    [FixedSize(3)]
    public class Codec
    {
        [Offset(0)]
        public SampleRate SampleRate { get; set; }

        [Offset(1)]
        public Encoding Encoding { get; set; }

        [Offset(2)]
        public byte Flags { get; set; }

        public int SampleRateInt
        {
            get
            {
                switch (SampleRate)
                {
                    case SampleRate.x22050Hz:
                        return 22050;
                    case SampleRate.x32000Hz:
                        return 32000;
                    case SampleRate.x44100Hz:
                        return 44100;
                    default:
                        throw new NotSupportedException("Sample Rate not supported");
                }
            }
        }

        public byte[] ChannelCounts
        {
            get
            {
                switch (Encoding)
                {
                    case Encoding.Mono:
                        return new byte[] { 1 };
                    case Encoding.Stereo:
                        return new byte[] { 2 };
                    case Encoding.Surround:
                        return new byte[] { 2, 2 };
                    case Encoding.Surround5_1:
                        return new byte[] { 2, 2, 2 };
                    default:
                        throw new NotSupportedException("Encoding not supported");
                }
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
    public class Playback
    {
        [Offset(0)]
        [StoreType(typeof(ushort))]
        public int NameIndex { get; set; }

        [Offset(2)]
        public short ParametersIndex { get; set; }

        [Offset(4)]
        public short Unknown { get; set; }

        [Offset(6)]
        public short FirstRuntimePermFlagIndex { get; set; }

        [Offset(8)]
        public short EncodedPermutationData { get; set; }

        [Offset(10)]
        [StoreType(typeof(ushort))]
        public int FirstPermutationIndex { get; set; }

        public int PermutationCount => (EncodedPermutationData >> 4) & 63;
    }

    [FixedSize(16, MaxVersion = (int)CacheType.HaloReachRetail)]
    [FixedSize(20, MinVersion = (int)CacheType.HaloReachRetail)]
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
    public class DataBlock
    {
        [Offset(0)]
        public int FileOffset { get; set; }

        [Offset(4)]
        public byte Flags { get; set; }

        //byte here that belongs to size (24bit)

        [Offset(6)]
        [StoreType(typeof(ushort))]
        public int Size { get; set; }

        [Offset(8)]
        public int RuntimeIndex { get; set; }

        [Offset(12)]
        public int Unknown0 { get; set; }

        [Offset(16)]
        public int Unknown1 { get; set; }
    }
}
