using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Audio
{
    public class XboxAdpcmHeader : IFormatHeader
    {
        const short formatId = 0x0069;

        public int Length => 20;

        public short NumChannels { get; }
        public int SampleRate { get; }
        public int ByteRate { get; }
        public short BlockAlign { get; }
        public short BitsPerSample { get; }
        public int ExtraData { get; }

        public XboxAdpcmHeader(int sampleRate, byte numChannels)
        {
            NumChannels = numChannels;
            SampleRate = sampleRate;
            BlockAlign = (short)(36 * numChannels);
            ByteRate = sampleRate * BlockAlign >> 6;
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
                sw.Write(NumChannels);
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
