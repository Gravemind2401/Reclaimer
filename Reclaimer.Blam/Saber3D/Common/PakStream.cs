using Reclaimer.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Reclaimer.Saber3D.Common
{
    public class PakStream : Stream
    {
        private record struct Chunk(int CompressedAddress, int CompressedSize, int UncompressedAddress, int UncompressedSize);

        private readonly Stream baseStream;
        private readonly Chunk[] chunks;

        private bool positionDirty;
        private ZLibStream chunkStream;

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

        public PakStream(string filePath)
            : this(new FileStream(filePath, FileMode.Open, FileAccess.Read))
        { }

        public PakStream(Stream baseStream)
        {
            if (!baseStream.CanRead || !baseStream.CanSeek)
                throw new NotSupportedException();

            this.baseStream = baseStream;

            using var reader = new EndianReader(baseStream, ByteOrder.LittleEndian, true);

            reader.Seek(0, SeekOrigin.Begin);

            var chunkCount = reader.ReadInt32();
            chunks = new Chunk[chunkCount];

            var offsets = new int[chunkCount + 1];
            offsets[^1] = (int)baseStream.Length;

            for (var i = 0; i < chunkCount; i++)
                offsets[i] = reader.ReadInt32();

            var dataAddress = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                reader.Seek(offsets[i], SeekOrigin.Begin);
                var dataSize = reader.ReadInt32();
                var chunkAddress = (int)reader.Position;
                var chunkSize = offsets[i + 1] - chunkAddress;

                chunks[i] = new Chunk(chunkAddress, chunkSize, dataAddress, dataSize);
                dataAddress += dataSize;
            }
            
            //attempt to correct for x360 files except they arent zlib?
            /*if (chunks.Any(c => c.UncompressedSize < 0 || c.UncompressedSize > 0x007FFFFF))
            {
                using var buffer = MemoryPool<byte>.Shared.Rent(0x40000);

                int bytesRead;
                for (var i = 0; i < chunkCount; i++)
                {
                    var c = chunks[i];
                    c.CompressedSize += sizeof(int);
                    c.UncompressedSize = 0;
                    c.CompressedAddress -= sizeof(int);

                    reader.Seek(c.CompressedAddress, SeekOrigin.Begin);
                    using var ms = new MemoryStream(reader.ReadBytes(c.CompressedSize));
                    using var ds = new ZLibStream(ms, CompressionMode.Decompress);

                    do { c.UncompressedSize += bytesRead = ds.Read(buffer.Memory.Span); }
                    while (bytesRead == buffer.Memory.Length);

                    chunks[i] = c;
                }
            }*/

            Length = chunks.Sum(c => c.UncompressedSize);
        }

        private void ChunkSeek(int bytesToSkip)
        {
            if (bytesToSkip == 0)
                return;

            var buffer = new byte[0x40000];
            do
            {
                bytesToSkip -= chunkStream.Read(buffer, 0, Math.Min(bytesToSkip, buffer.Length));
            }
            while (bytesToSkip > 0);
        }

        private void LoadChunk()
        {
            //TODO:if position has moved forward within the same chunk, just use ChunkSeek() instead of reloading the chunk

            UnloadChunk();

            var nextChunk = chunks.SkipWhile(c => c.UncompressedAddress > position).First();
            baseStream.Seek(nextChunk.CompressedAddress, SeekOrigin.Begin);

            var chunkData = new byte[nextChunk.CompressedSize];
            baseStream.ReadAll(chunkData, 0, chunkData.Length);

            var ms = new MemoryStream(chunkData);
            chunkStream = new ZLibStream(ms, CompressionMode.Decompress);

            //if position is part way through the chunk we need to consume the deflate stream until we reach the correct position
            var seekBytes = (int)position - nextChunk.UncompressedAddress;
            ChunkSeek(seekBytes);
        }

        private void UnloadChunk()
        {
            chunkStream?.Dispose();
            chunkStream = null;
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

            positionDirty = offset != position;
            return position = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0 || offset >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (position >= Length)
                return 0;

            var bytesRemaining = count;

            do
            {
                if (chunkStream == null || positionDirty)
                    LoadChunk();

                var bytesRead = chunkStream.Read(buffer, offset, bytesRemaining);
                bytesRemaining -= bytesRead;
                position += bytesRead;

                //we reached the end of the chunk and need to load the next one
                if (bytesRemaining > 0)
                    positionDirty = true;
            }
            while (position < Length && bytesRemaining > 0);

            return count - bytesRemaining;
        }

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
    }
}
