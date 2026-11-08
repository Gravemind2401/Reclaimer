using Reclaimer.IO;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Reclaimer.Blam.Halo1
{
    public sealed class XboxCompressedCacheStream : Stream
    {
        private static readonly ConditionalWeakTable<CacheFile, byte[]> uncompressedDataCache = new();

        private readonly byte[] uncompressedData;

        private readonly Stream baseStream;
        private readonly bool leaveOpen;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length { get; }

        private long position;
        public override long Position
        {
            get => position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public XboxCompressedCacheStream(CacheFile cacheFile)
            : this(cacheFile, new FileStream(cacheFile.FileName, FileMode.Open, FileAccess.Read), false)
        { }

        public XboxCompressedCacheStream(CacheFile cacheFile, Stream baseStream)
            : this(cacheFile, baseStream, false)
        { }

        public XboxCompressedCacheStream(CacheFile cacheFile, Stream baseStream, bool leaveOpen)
        {
            ArgumentNullException.ThrowIfNull(baseStream);

            if (!baseStream.CanRead || !baseStream.CanSeek)
                throw new NotSupportedException($"{nameof(baseStream)} must be readable and seekable");

            this.baseStream = baseStream;
            this.leaveOpen = leaveOpen;

            if (uncompressedDataCache.TryGetValue(cacheFile, out uncompressedData))
            {
                Length = uncompressedData.Length;
                return;
            }

            var headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
            var uncompressedDataSize = cacheFile.Header.FileSize - headerSize;
            var initialPosition = baseStream.Position;

            uncompressedData = new byte[headerSize + uncompressedDataSize];
            baseStream.Position = 0;
            baseStream.ReadExactly(uncompressedData, 0, headerSize);

            baseStream.Position += 2; // ??
            using (var ds = new DeflateStream(baseStream, CompressionMode.Decompress, true))
                ds.ReadExactly(uncompressedData, headerSize, uncompressedDataSize);

            Length = uncompressedData.Length;
            uncompressedDataCache.Add(cacheFile, uncompressedData);

            baseStream.Position = initialPosition;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            offset = origin switch
            {
                SeekOrigin.Current => position + offset,
                SeekOrigin.End => Length + offset,
                _ => offset
            };

            if (offset < 0 || offset >= Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            return position = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (Position < 0)
                throw new InvalidOperationException("Attempted to read before the beginning of the stream");

            if (Position >= Length)
                return 0;

            var bytesRead = (int)Math.Min(count, Length - Position);
            Array.Copy(uncompressedData, Position, buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                if (disposing && !leaveOpen)
                    baseStream?.Dispose();
            }
        }
    }
}
