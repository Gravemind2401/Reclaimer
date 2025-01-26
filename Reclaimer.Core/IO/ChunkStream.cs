using System.IO;

namespace Reclaimer.IO
{
    public abstract class ChunkStream : Stream
    {
        private readonly ChunkTracker chunkTracker;
        private readonly bool leaveOpen;

        private ChunkAddressMapping[] chunks;

        //set initial value to true to ensure first read triggers a chunk update
        private bool positionDirty = true;

        protected Stream BaseStream { get; }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        private long length;
        public sealed override long Length => length;

        private long? lastActualPosition;
        private long position;
        public sealed override long Position
        {
            get => position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public ChunkStream(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read))
        { }

        public ChunkStream(Stream baseStream)
            : this (baseStream, false)
        { }

        public ChunkStream(Stream baseStream, bool leaveOpen)
        {
            ArgumentNullException.ThrowIfNull(baseStream);

            if (!baseStream.CanRead || !baseStream.CanSeek)
                throw new NotSupportedException($"{nameof(baseStream)} must be readable and seekable");

            BaseStream = baseStream;
            this.leaveOpen = leaveOpen;
            chunkTracker = new ChunkTracker(this);
        }

        protected void InitializeChunks()
        {
            if (chunks != null)
                return;

            var chunkDetails = ReadChunks();
            if (chunkDetails == null || chunkDetails.Count == 0)
                throw new InvalidDataException("Stream must contain at least one chunk");

            chunks = new ChunkAddressMapping[chunkDetails.Count];

            var destAddress = 0;
            for (var i = 0; i < chunkDetails.Count; i++)
            {
                var (sourceAddress, compressedSize, uncompressedSize) = chunkDetails[i];
                chunks[i] = new ChunkAddressMapping(sourceAddress, compressedSize, destAddress, uncompressedSize);
                destAddress += uncompressedSize;
            }

            length = chunks.Sum(c => c.UncompressedSize);

            chunkTracker.PrepareBuffer();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            InitializeChunks();

            offset = origin switch
            {
                SeekOrigin.Current => position + offset,
                SeekOrigin.End => Length + offset,
                _ => offset
            };

            if (offset < 0 || offset >= Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset != position && !lastActualPosition.HasValue)
            {
                //if position was manually changed we need to refresh the current chunk on next read
                positionDirty = true;
                lastActualPosition = position;
            }
            else if (offset == lastActualPosition)
            {
                positionDirty = false;
                lastActualPosition = null;
            }

            return position = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            InitializeChunks();

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (position < 0)
                throw new InvalidOperationException("Attempted to read before the beginning of the stream");

            if (position >= Length)
                return 0;

            if (positionDirty)
            {
                //save time by using a dirty flag instead of looking up and comparing the current chunk every read
                chunkTracker.PrepareChunk();
                positionDirty = false;
                lastActualPosition = null;
            }

            var bytesRemaining = count;

            do
            {
                var bytesRead = chunkTracker.ChunkStream.Read(buffer, offset, bytesRemaining);
                bytesRemaining -= bytesRead;
                position += bytesRead;
                offset += bytesRead;

                if (chunkTracker.IsEndOfChunk)
                    chunkTracker.PrepareChunk();
                else if (bytesRead == 0)
                    break;
            }
            while (position < Length && bytesRemaining > 0);

            return count - bytesRemaining;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();

        protected abstract IList<ChunkLocator> ReadChunks();
        protected abstract Stream CreateDecompressionStream(Stream sourceStream, bool leaveOpen, int compressedSize, int uncompressedSize);

        protected record struct ChunkLocator(int SourceAddress, int CompressedSize, int UncompressedSize);

        private record struct ChunkAddressMapping(int SourceAddress, int CompressedSize, int DestAddress, int UncompressedSize)
        {
            public readonly bool ContainsAddress(int address) => address >= DestAddress && address < DestAddress + UncompressedSize;
        }

        private sealed class ChunkTracker
        {
            private readonly ChunkStream sourceStream;

            public ChunkAddressMapping CurrentChunk { get; private set; }
            public MemoryStream ChunkStream { get; private set; }

            public long InnerPosition => sourceStream.Position - CurrentChunk.DestAddress;
            public bool IsEndOfChunk => !CurrentChunk.ContainsAddress((int)sourceStream.Position);

            public ChunkTracker(ChunkStream sourceStream)
            {
                this.sourceStream = sourceStream;
            }

            public void PrepareBuffer()
            {
                var capacity = sourceStream.chunks.Max(c => c.UncompressedSize);
                ChunkStream = new MemoryStream(capacity);
            }

            public void PrepareChunk()
            {
                var nextChunk = sourceStream.chunks.First(c => c.ContainsAddress((int)sourceStream.Position));
                if (nextChunk != CurrentChunk)
                {
                    ChunkStream.Position = 0;
                    ChunkStream.SetLength(nextChunk.UncompressedSize);

                    sourceStream.BaseStream.Seek(nextChunk.SourceAddress, SeekOrigin.Begin);
                    using (var expandStream = sourceStream.CreateDecompressionStream(sourceStream.BaseStream, true, nextChunk.CompressedSize, nextChunk.UncompressedSize))
                        expandStream.ReadExactly(ChunkStream.GetBuffer(), 0, (int)ChunkStream.Length);

                    CurrentChunk = nextChunk;
                }

                ChunkStream.Position = InnerPosition;
            }

            public void Dispose()
            {
                ChunkStream?.Close();
                ChunkStream?.Dispose();
                ChunkStream = null;
                CurrentChunk = default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                chunkTracker.Dispose();
                base.Dispose(disposing);
            }
            finally
            {
                if (disposing && !leaveOpen)
                    BaseStream?.Dispose();
            }
        }
    }
}
