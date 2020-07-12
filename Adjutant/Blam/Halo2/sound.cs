using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adjutant.Audio;
using Adjutant.Blam.Common;

namespace Adjutant.Blam.Halo2
{
    public class sound //: ISoundContainer
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public sound(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(0)]
        public short Flags { get; set; }

        [Offset(2)]
        public byte SoundClass { get; set; }

        [Offset(3)]
        public SampleRate SampleRate { get; set; }

        [Offset(4)]
        public byte Encoding { get; set; }

        [Offset(5)]
        public CompressionCodec CompressionCodec { get; set; }

        [Offset(6)]
        public short PlaybackIndex { get; set; }

        [Offset(8)]
        public short PitchRangeIndex { get; set; }

        [Offset(11)]
        public byte ScaleIndex { get; set; }

        [Offset(12)]
        public byte PromotionIndex { get; set; }

        [Offset(13)]
        public byte CustomPlaybackIndex { get; set; }

        [Offset(14)]
        public short ExtraInfoIndex { get; set; }

        [Offset(16)]
        public int MaxPlaytime { get; set; }

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
    }

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

    public enum CompressionCodec : byte
    {
        BigEndian = 0,
        XboxAdpcm = 1,
        ImaAdpcm = 2,
        LittleEndian = 3,
        WMA = 4
    }
}
