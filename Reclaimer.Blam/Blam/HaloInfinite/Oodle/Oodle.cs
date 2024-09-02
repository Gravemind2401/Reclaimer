﻿using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace OodleSharp
{
    public static class Oodle
    {
        [DllImport("oo2core_8_win64.dll")]
        private static extern int OodleLZ_Compress(OodleFormat format, byte[] buffer, long bufferSize, byte[] outputBuffer, OodleCompressionLevel level, uint options, uint offs, uint unk, byte[] scratchBuffer, uint scratchBufferSize);

        [DllImport("oo2core_8_win64.dll")]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize,
            uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        public static byte[] Compress(byte[] buffer, int size, OodleFormat format, OodleCompressionLevel level)
        {
            byte[] skBuffer = new byte[0];
            uint skBufferSize = (uint)skBuffer.Length;
            uint compressedBufferSize = GetCompressionBound((uint)size);
            byte[] compressedBuffer = new byte[compressedBufferSize];

            int compressedCount = OodleLZ_Compress(format, buffer, size, compressedBuffer, level, 0, 0, 0, skBuffer, skBufferSize);

            byte[] outputBuffer = new byte[compressedCount];
            Buffer.BlockCopy(compressedBuffer, 0, outputBuffer, 0, compressedCount);

            return outputBuffer;
        }

        public static byte[] Decompress(byte[] buffer, int size, int uncompressedSize)
        {
            byte[] decompressedBuffer = new byte[uncompressedSize];
            int decompressedCount = OodleLZ_Decompress(buffer, size, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            if (decompressedCount == uncompressedSize)
            {
                return decompressedBuffer;
            }
            else if (decompressedCount < uncompressedSize)
            {
                return decompressedBuffer;
            }
            else
            {
                throw new Exception("There was an error while decompressing");
            }
        }

        private static uint GetCompressionBound(uint bufferSize)
        {
            return bufferSize + 274 * ((bufferSize + 0x3FFFF) / 0x40000);
        }
    }

    public enum OodleCompressionLevel : ulong
    {
        None,
        SuperFast,
        VeryFast,
        Fast,
        Normal,
        Optimal1,
        Optimal2,
        Optimal3,
        Optimal4,
        Optimal5
    }

    public enum OodleFormat : uint
    {
        LZH,
        LZHLW,
        LZNIB,
        None,
        LZB16,
        LZBLW,
        LZA,
        LZNA,
        Kraken,
        Mermaid,
        BitKnit,
        Selkie,
        Akkorokamui
    }
}