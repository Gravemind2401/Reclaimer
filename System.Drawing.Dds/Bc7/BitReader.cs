using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds.Bc7
{
    internal class BitReader
    {
        private const int byteSize = 8;
        private const int cacheSize = 64;

        private readonly byte[] stream;

        private long cacheBits;
        private int cachePosition;

        private int StreamIndex => Position / byteSize;
        private int CacheProgress => Position - cachePosition;

        public bool EOF => Position > stream.Length * byteSize;

        private int position;
        public int Position
        {
            get { return position; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                position = value;
                if (position < cachePosition || position >= cachePosition + cacheSize)
                    SetCache(StreamIndex);
            }
        }

        public BitReader(byte[] stream)
        {
            this.stream = stream;
            SetCache(0);
        }

        public byte ReadBit() => ReadBits(1);

        public byte ReadBits(byte bits)
        {
            if (bits < 0 || bits > 8)
                throw new ArgumentOutOfRangeException(nameof(bits));

            if (bits == 0)
                return 0;

            var result = (byte)ReadBitsInternal(bits);
            return result;
        }

        private int ReadBitsInternal(int bits)
        {
            if (CacheProgress + bits >= cacheSize)
                SetCache(StreamIndex);

            var mask = (1 << bits) - 1;

            var value = (int)(cacheBits >> CacheProgress) & mask; //RTL
            //var value = (int)(cacheBits >> cacheSize - (CacheProgress + bits)) & mask; //LTR

            Position += bits;
            return value;
        }

        private void SetCache(int offset)
        {
            cacheBits = 0;
            cachePosition = offset * byteSize;

            for (int i = 0; i < cacheSize / byteSize; i++)
            {
                if (offset + i >= stream.Length)
                    break;

                var shift = i; //RTL
                //var shift = 7 - i; //LTR (MSB-first)

                cacheBits |= ((long)stream[offset + i] << byteSize * shift);
            }
        }
    }
}
