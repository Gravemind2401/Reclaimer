using Reclaimer.IO;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Reclaimer.Blam.Halo2
{
    public class MccCompressedCacheStream : ChunkStream
    {
        private static readonly ConditionalWeakTable<CacheFile, ChunkLocator[]> chunkTableCache = new();

        private readonly CacheFile cache;
        private readonly int headerSize;

        public MccCompressedCacheStream(CacheFile cacheFile)
            : base(cacheFile.FileName)
        {
            cache = cacheFile;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
        }

        public MccCompressedCacheStream(CacheFile cacheFile, Stream baseStream)
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

            chunks[0] = new ChunkLocator(0, null, headerSize);

            var tempBuffer = ArrayPool<byte>.Shared.Rent(cache.Header.CompressedDataChunkSize);

            for (var i = 0; i < chunkCount; i++)
            {
                var chunkSize = (int?)reader.ReadInt32();
                var chunkAddress = reader.ReadInt32();
                var uncompressedSize = cache.Header.CompressedDataChunkSize;

                if (chunkSize < 0)
                {
                    uncompressedSize = -chunkSize.Value;
                    chunkSize = null;
                    chunkAddress += 2; //skip chunk header
                }
                else
                {
                    //decompress each chunk to find the actual size, as it may be less than the target size
                    var originalPosition = reader.Position;
                    reader.Seek(chunkAddress, SeekOrigin.Begin);
                    using (var ds = new ZLibStream(BaseStream, CompressionMode.Decompress, true))
                        uncompressedSize = ds.ReadAll(tempBuffer);
                    reader.Seek(originalPosition, SeekOrigin.Begin);
                }

                chunks[i + 1] = new ChunkLocator(chunkAddress, chunkSize, uncompressedSize);
            }

            ArrayPool<byte>.Shared.Return(tempBuffer);

            chunkTableCache.Add(cache, chunks);

            return chunks;
        }

        protected override Stream CreateDecompressionStream(Stream sourceStream, bool leaveOpen, int? compressedSize, int uncompressedSize)
        {
            return compressedSize.HasValue
                ? new ZLibStream(sourceStream, CompressionMode.Decompress, leaveOpen)
                : sourceStream;
        }
    }
}
