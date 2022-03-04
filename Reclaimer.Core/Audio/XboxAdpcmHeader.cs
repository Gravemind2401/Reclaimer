using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Audio
{
    //http://wiki.xentax.com/index.php/Psychonauts_ISB
    //https://xboxdevwiki.net/Xbox_ADPCM
    //https://github.com/Sergeanur/XboxADPCM/blob/master/XboxADPCM/XboxADPCM.cpp
    //http://samples.ffmpeg.org/game-formats/xbox-adpcm-wav/
    public class XboxAdpcmHeader : IFormatHeader
    {
        private const short formatId = 0x0069;

        public int Length => 20;

        public short ChannelCount { get; }
        public int SampleRate { get; }
        public int ByteRate { get; }
        public short BlockAlign { get; }
        public short BitsPerSample { get; }
        public int ExtraData { get; }

        public XboxAdpcmHeader(int sampleRate, byte channelCount)
        {
            ChannelCount = channelCount;
            SampleRate = sampleRate;
            BlockAlign = (short)(36 * ChannelCount);
            ByteRate = SampleRate * BlockAlign >> 6;
            BitsPerSample = 4;
            ExtraData = 0x00400002;
        }

        public byte[] GetBytes()
        {
            var buffer = new byte[Length];

            using (var ms = new MemoryStream(buffer))
            using (var sw = new EndianWriter(ms, ByteOrder.LittleEndian))
            {
                sw.Write(formatId);
                sw.Write(ChannelCount);
                sw.Write(SampleRate);
                sw.Write(ByteRate);
                sw.Write(BlockAlign);
                sw.Write(BitsPerSample);
                sw.Write(ExtraData);
            }

            return buffer;
        }
    }
}
