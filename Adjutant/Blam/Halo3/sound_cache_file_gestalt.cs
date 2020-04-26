using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class sound_cache_file_gestalt
    {
        [Offset(0)]
        public BlockCollection<Codec> Codecs { get; set; }

        [Offset(36)]
        public BlockCollection<SoundName> SoundNames { get; set; }

        [Offset(60)]
        public BlockCollection<Playback> Playbacks { get; set; }

        [Offset(72)]
        public BlockCollection<SoundPermutation> SoundPermutations { get; set; }

        [Offset(148)]
        public BlockCollection<DataBlock> DataBlocks { get; set; }
    }

    [FixedSize(3)]
    public class Codec
    {
        [Offset(0)]
        public byte Unknown { get; set; }

        [Offset(1)]
        public SoundType SoundType { get; set; }

        [Offset(2)]
        public byte Flags { get; set; }

        public byte ChannelCount
        {
            get
            {
                switch (SoundType)
                {
                    case SoundType.Mono:
                        return 1;
                    case SoundType.Stereo:
                        return 2;
                    default:
                        throw new NotSupportedException("Sound type not supported");
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
        public short FirstPermutation { get; set; }

        public int PermutationCount => (EncodedPermutationData >> 4) & 63;
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
    public class DataBlock
    {
        [Offset(0)]
        public int FileOffset { get; set; }

        [Offset(4)]
        public short Flags { get; set; }

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

    public enum SoundType : byte
    {
        Mono = 0,
        Stereo = 1,
        Unknown2 = 2, //2 and 3 probably surround stereo
        Unknown3 = 3
    }
}
