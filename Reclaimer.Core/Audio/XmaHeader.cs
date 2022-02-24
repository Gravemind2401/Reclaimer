using System;
using System.Collections.Generic;
using System.IO;
using Reclaimer.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Audio
{
    public class XmaHeader : IFormatHeader
    {
        const short formatId = 0x0165;

        public int Length => 12 + Streams.Count * 20;

        public short BitsPerSample { get; set; }
        public List<XmaStreamInfo> Streams { get; }

        public XmaHeader(int sampleRate, byte[] channelCounts)
        {
            if (channelCounts == null)
                throw new ArgumentNullException(nameof(channelCounts));

            BitsPerSample = 16;
            Streams = new List<XmaStreamInfo>();

            foreach (var channelCount in channelCounts)
            {
                Streams.Add(new XmaStreamInfo
                {
                    SampleRate = sampleRate,
                    ChannelCount = channelCount
                });
            }
        }

        //fields below with a fixed value are not used by ffmpeg
        public byte[] GetBytes()
        {
            var buffer = new byte[Length];

            using (var ms = new MemoryStream(buffer))
            using (var sw = new EndianWriter(ms, ByteOrder.LittleEndian))
            {
                sw.Write(formatId);
                sw.Write(BitsPerSample);
                sw.Write((short)0); //encode options
                sw.Write((short)0); //largest skip
                sw.Write((short)Streams.Count);
                sw.Write((byte)0); //loops
                sw.Write((byte)3); //encoder version

                foreach (var s in Streams)
                {
                    sw.Write(0); //bytes per second
                    sw.Write(s.SampleRate);
                    sw.Write(0); //loop start
                    sw.Write(0); //loop end
                    sw.Write((byte)0); //subframe loop data
                    sw.Write(s.ChannelCount);
                    sw.Write((short)2); //channel mask: L, R, C, LFE, LSur, RSur, LB, RB
                }
            }

            return buffer;
        }
    }

    public class XmaStreamInfo
    {
        public int SampleRate { get; set; }
        public byte ChannelCount { get; set; }
    }
}
