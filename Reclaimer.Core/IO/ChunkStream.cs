using System.Buffers;
using System.IO;

namespace Reclaimer.IO
{
    public abstract class ChunkStream : Stream
    {
        private readonly ChunkAddressMapping[] chunks;
        private readonly ChunkTracker chunkTracker;

        //set initial value to true to ensure first read triggers a chunk update
        private bool positionDirty = true;

        protected Stream BaseStream { get; }
        public override long Length { get; }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

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
        {
            ArgumentNullException.ThrowIfNull(baseStream);

            if (!baseStream.CanRead || !baseStream.CanSeek)
                throw new NotSupportedException($"{nameof(baseStream)} must be readable and seekable");

            BaseStream = baseStream;
            chunkTracker = new ChunkTracker(this);

            var chunkDetails = ReadChunks();
            chunks = new ChunkAddressMapping[chunkDetails.Count];

            var destAddress = 0;
            for (var i = 0; i < chunkDetails.Count; i++)
            {
                var (sourceAddress, compressedSize, uncompressedSize) = chunkDetails[i];
                chunks[i] = new ChunkAddressMapping(sourceAddress, compressedSize, destAddress, uncompressedSize);
                destAddress += uncompressedSize;
            }

            Length = chunks.Sum(c => c.UncompressedSize);
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

            //if position was manually changed we need to refresh the current chunk on next read
            positionDirty = offset != position;

            return position = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

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
        protected abstract Stream GetChunkStream(byte[] chunkData);

        protected record struct ChunkLocator(int SourceAddress, int CompressedSize, int UncompressedSize);

        private record struct ChunkAddressMapping(int SourceAddress, int CompressedSize, int DestAddress, int UncompressedSize)
        {
            public readonly bool ContainsAddress(int address) => address >= DestAddress && address < DestAddress + UncompressedSize;
        }

        private sealed class ChunkTracker
        {
            private readonly ChunkStream sourceStream;

            public ChunkAddressMapping CurrentChunk { get; private set; }
            public byte[] CompressedData { get; private set; }
            public Stream ChunkStream { get; private set; }

            public long InnerPosition => sourceStream.Position - CurrentChunk.DestAddress;
            public bool IsEndOfChunk => !CurrentChunk.ContainsAddress((int)sourceStream.Position);

            public ChunkTracker(ChunkStream sourceStream)
            {
                this.sourceStream = sourceStream;
            }

            public void PrepareChunk()
            {
                var nextChunk = sourceStream.chunks.First(c => c.ContainsAddress((int)sourceStream.Position));
                if (nextChunk != CurrentChunk)
                {
                    CloseChunk();

                    sourceStream.BaseStream.Seek(nextChunk.SourceAddress, SeekOrigin.Begin);

                    CompressedData = new byte[nextChunk.UncompressedSize];
                    sourceStream.BaseStream.ReadAll(CompressedData, 0, CompressedData.Length);

                    CurrentChunk = nextChunk;
                }

                //always reload the chunk stream to make sure it starts at 0 again
                ChunkStream = sourceStream.GetChunkStream(CompressedData);

                if (InnerPosition == 0)
                    return;
                else if (ChunkStream.CanSeek)
                {
                    ChunkStream.Position = InnerPosition;
                    return;
                }

                //the only way to move forward now is to read until we get to the desired position
                var remaining = InnerPosition;
                int bytesRead;
                do
                {
                    var bufferSize = Math.Min((int)remaining, 0x10000);
                    var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);
                    var span = buffer.Memory.Span[..bufferSize]; //in case we got more than we wanted

                    remaining -= bytesRead = ChunkStream.ReadAll(span);
                }
                while (remaining > 0 && bytesRead > 0);
            }

            public void CloseChunk()
            {
                ChunkStream?.Close();
                ChunkStream = null;
                CompressedData = null;
                CurrentChunk = default;
            }
        }

        protected override void Dispose(bool disposing)
        {
            chunkTracker.CloseChunk();
            base.Dispose(disposing);
        }
    }
}
