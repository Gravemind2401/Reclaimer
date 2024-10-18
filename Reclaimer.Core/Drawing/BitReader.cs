namespace Reclaimer.Drawing.Bc7
{
    internal ref struct BitReader
    {
        private const int byteSize = 8;
        private const int cacheSize = 64;
        private const int bytesInCache = cacheSize / byteSize;

        private readonly byte[] stream;

        private long cacheBits;
        private int cachePosition;

        private int cacheProgress;

        private int StreamIndex => Position / byteSize;
        private int CacheProgress => Position - cachePosition;

        public bool EOF => Position > stream.Length * byteSize;

        private int position;
        public int Position
        {
            get => position;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                position = value;
                cacheProgress = value - cachePosition;

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

            if (cacheProgress + bits >= cacheSize)
                SetCache(StreamIndex);

            var mask = (1 << bits) - 1;
            var value = (int)(cacheBits >> cacheProgress) & mask; //RTL
            //var value = (int)(cacheBits >> cacheSize - (cacheProgress + bits)) & mask; //LTR

            Position += bits;
            return (byte)value;
        }

        private void SetCache(int offset)
        {
            cacheBits = 0;
            cachePosition = offset * byteSize;
            cacheProgress = Position - cachePosition;

            for (var i = 0; i < bytesInCache; i++)
            {
                if (offset + i >= stream.Length)
                    break;

                var shift = i; //RTL
                //var shift = 7 - i; //LTR (MSB-first)

                cacheBits |= (long)stream[offset + i] << byteSize * shift;
            }
        }
    }
}
