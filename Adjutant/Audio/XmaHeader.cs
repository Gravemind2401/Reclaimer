using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Audio
{
    public class XmaHeader : IFormatHeader
    {
        const short formatId = 0x0165;

        public int Length => 12 + Streams.Count * 20;

        public short BitsPerSample { get; set; }
        public List<XmaStreamInfo> Streams { get; }

        public XmaHeader(int sampleRate, byte channelCount)
        {
            BitsPerSample = 16;
            Streams = new List<XmaStreamInfo>();
            Streams.Add(new XmaStreamInfo
            {
                SampleRate = sampleRate,
                ChannelCount = channelCount
            });
        }

        public byte[] GetBytes()
        {
            var buffer = new byte[Length];

            using (var ms = new MemoryStream(buffer))
            using (var sw = new EndianWriter(ms, ByteOrder.LittleEndian))
            {
                sw.Write(formatId);
                sw.Write(BitsPerSample);
                sw.Write((short)0);
                sw.Write((short)0);
                sw.Write((short)Streams.Count);
                sw.Write((byte)0);
                sw.Write((byte)3);

                foreach (var s in Streams)
                {
                    sw.Write(0);
                    sw.Write(s.SampleRate);
                    sw.Write(0);
                    sw.Write(0);
                    sw.Write((byte)0);
                    sw.Write(s.ChannelCount);
                    sw.Write((short)2);
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
