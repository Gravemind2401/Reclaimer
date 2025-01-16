using Reclaimer.IO;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Reclaimer.Blam.Halo2
{
    public class MccCacheDeflateStream : ChunkStream
    {
        private static readonly ConditionalWeakTable<CacheFile, ChunkLocator[]> chunkTableCache = new();

        private readonly CacheFile cache;
        private readonly int headerSize;

        public MccCacheDeflateStream(CacheFile cacheFile)
            : base(cacheFile.FileName)
        {
            cache = cacheFile;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
        }

        public MccCacheDeflateStream(CacheFile cacheFile, Stream baseStream)
            : base(baseStream)
        {
            cache = cacheFile;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
        }

        protected override IList<ChunkLocator> ReadChunks()
        {
            if (chunkTableCache.TryGetValue(cache, out var chunks))
                return chunks;

            using var reader = new EndianReader(BaseStream, ByteOrder.LittleEndian, true);

            var chunkCount = cache.Header.CompressedChunkCount;
            chunks = new ChunkLocator[chunkCount + 1];

            reader.Seek(cache.Header.CompressedChunkTableAddress, SeekOrigin.Begin);

            chunks[0] = new ChunkLocator(0, headerSize, headerSize);

            for (var i = 0; i < chunkCount; i++)
            {
                var chunkSize = reader.ReadInt32();
                var chunkAddress = reader.ReadInt32() + 2; //skip whatever this value is - its not part of the stream

                chunks[i + 1] = new ChunkLocator(chunkAddress, chunkSize, cache.Header.CompressedDataChunkSize);
            }

            chunkTableCache.Add(cache, chunks);

            return chunks;
        }

        protected override Stream CreateDecompressionStream(Stream sourceStream, bool leaveOpen)
        {
            return Position < headerSize
                ? sourceStream
                : new DeflateStream(sourceStream, CompressionMode.Decompress, leaveOpen);
        }
    }
}
