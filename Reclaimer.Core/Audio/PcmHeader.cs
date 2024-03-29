﻿using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Audio
{
    public class PcmHeader : IFormatHeader
    {
        private const short formatId = 0x0001;

        public int Length => 16;

        public short ChannelCount { get; }
        public int SampleRate { get; }
        public int ByteRate { get; }
        public short BlockAlign { get; }
        public short BitsPerSample { get; }

        public PcmHeader(int sampleRate, byte channelCount)
        {
            ChannelCount = channelCount;
            SampleRate = sampleRate;
            BlockAlign = (short)(2 * ChannelCount);
            BitsPerSample = 16;
            ByteRate = ChannelCount * SampleRate * (BitsPerSample / 8);
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
            }

            return buffer;
        }
    }
}
