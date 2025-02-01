using Reclaimer.IO;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Reclaimer.Blam.Halo1
{
    public class XboxCompressedCacheStream : ChunkStream
    {
        private static readonly ConditionalWeakTable<CacheFile, ChunkLocator[]> chunkTableCache = new();

        private readonly CacheFile cache;
        private readonly int headerSize;
        private readonly int totalCompressedSize;

        public XboxCompressedCacheStream(CacheFile cacheFile)
            : base(cacheFile.FileName)
        {
            cache = cacheFile;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
            totalCompressedSize = (int)new FileInfo(cacheFile.FileName).Length;
        }

        public XboxCompressedCacheStream(CacheFile cacheFile, Stream baseStream)
            : base(baseStream)
        {
            cache = cacheFile;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
            totalCompressedSize = (int)new FileInfo(cacheFile.FileName).Length;
        }

        protected override IList<ChunkLocator> ReadChunks()
        {
            if (chunkTableCache.TryGetValue(cache, out var chunks))
                return chunks;

            var compressedDataSize = totalCompressedSize - headerSize;
            var uncompressedDataSize = cache.Header.FileSize - headerSize;

            chunks = new ChunkLocator[2];
            chunks[0] = new ChunkLocator(0, null, headerSize);
            chunks[1] = new ChunkLocator(headerSize + 2, compressedDataSize, uncompressedDataSize);

            chunkTableCache.Add(cache, chunks);

            return chunks;
        }

        protected override Stream CreateDecompressionStream(Stream sourceStream, bool leaveOpen, int? compressedSize, int uncompressedSize)
        {
            return compressedSize.HasValue
                ? new DeflateStream(sourceStream, CompressionMode.Decompress, leaveOpen)
                : sourceStream;
        }
    }
}
