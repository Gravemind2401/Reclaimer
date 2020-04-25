using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo3
{
    public class sound
    {
        [Offset(0)]
        public short Flags { get; set; }

        [Offset(2)]
        public byte SoundClass { get; set; }

        [Offset(3)]
        public SampleRate SampleRate { get; set; }

        [Offset(4)]
        public byte Encoding { get; set; }

        [Offset(5)]
        public byte CodecIndex { get; set; }

        [Offset(6)]
        public short PlaybackIndex { get; set; }

        [Offset(8)]
        public short DialogueUnknown { get; set; }

        [Offset(10)]
        public short Unknown0 { get; set; }

        [Offset(12)]
        public short PitchRangeIndex1 { get; set; }

        [Offset(14)]
        public byte PitchRangeIndex2 { get; set; }

        [Offset(15)]
        public byte ScaleIndex { get; set; }

        [Offset(16)]
        public byte PromotionIndex { get; set; }

        [Offset(17)]
        public byte CustomPlaybackIndex { get; set; }

        [Offset(18)]
        public short ExtraInfoIndex { get; set; }

        [Offset(20)]
        public int Unknown1 { get; set; }

        [Offset(24)]
        public ResourceIdentifier ResourceIdentifier { get; set; }

        [Offset(28)]
        public int MaxPlaytime { get; set; }
    }

    public enum SampleRate : byte
    {
        x22050Hz = 0,
        x44100Hz = 1
    }
}
