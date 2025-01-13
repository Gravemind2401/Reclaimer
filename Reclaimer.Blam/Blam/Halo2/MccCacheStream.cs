using Reclaimer.IO;
using System.IO;
using System.IO.Compression;

namespace Reclaimer.Blam.Halo2
{
    public class MccCacheStream : ChunkStream
    {
        private readonly CacheHeader header;
        private readonly int headerSize;

        public MccCacheStream(CacheFile cacheFile)
            : base(cacheFile.FileName)
        {
            header = cacheFile.Header;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
        }

        public MccCacheStream(CacheFile cacheFile, Stream baseStream)
            : base(baseStream)
        {
            header = cacheFile.Header;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
        }

        protected override IList<ChunkLocator> ReadChunks()
        {
            using var reader = new EndianReader(BaseStream, ByteOrder.LittleEndian, true);

            var chunkCount = header.CompressedChunkCount;
            var chunks = new ChunkLocator[chunkCount + 1];

            reader.Seek(header.CompressedChunkTableAddress, SeekOrigin.Begin);

            chunks[0] = new ChunkLocator(0, headerSize, headerSize);

            for (var i = 0; i < chunkCount; i++)
            {
                var chunkSize = reader.ReadInt32();
                var chunkAddress = reader.ReadInt32() + 2; //skip whatever this value is - its not part of the stream

                chunks[i + 1] = new ChunkLocator(chunkAddress, chunkSize, header.CompressedDataChunkSize);
            }

            return chunks;
        }

        protected override Stream GetChunkStream(byte[] chunkData)
        {
            return Position < headerSize
                ? new MemoryStream(chunkData)
                : new DeflateStream(new MemoryStream(chunkData), CompressionMode.Decompress);
        }
    }
}
