using Reclaimer.IO;
using System.IO;
using System.IO.Compression;

namespace Reclaimer.Blam.Halo1
{
    public class XboxCacheDeflateStream : ChunkStream
    {
        private readonly CacheHeader header;
        private readonly int headerSize;
        private readonly int totalCompressedSize;

        public XboxCacheDeflateStream(CacheFile cacheFile)
            : base(cacheFile.FileName)
        {
            header = cacheFile.Header;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
            totalCompressedSize = (int)new FileInfo(cacheFile.FileName).Length;
        }

        public XboxCacheDeflateStream(CacheFile cacheFile, Stream baseStream)
            : base(baseStream)
        {
            header = cacheFile.Header;
            headerSize = (int)FixedSizeAttribute.ValueFor(typeof(CacheHeader), (int)cacheFile.CacheType);
            totalCompressedSize = (int)new FileInfo(cacheFile.FileName).Length;
        }

        protected override IList<ChunkLocator> ReadChunks()
        {
            var compressedDataSize = totalCompressedSize - headerSize;
            var uncompressedDataSize = header.FileSize - headerSize;

            var chunks = new ChunkLocator[2];
            chunks[0] = new ChunkLocator(0, headerSize, headerSize);
            chunks[1] = new ChunkLocator(headerSize + 2, compressedDataSize, uncompressedDataSize);

            return chunks;
        }

        protected override Stream GetChunkStream(byte[] chunkData)
        {
            if (Position < headerSize)
                return new MemoryStream(chunkData);

            //this eats a lot of memory for repeated stream open/close, but it makes the resulting stream seekable
            //which is a necessity considering almost the entire map is in one big compressed block
            using (var ds = new DeflateStream(new MemoryStream(chunkData), CompressionMode.Decompress))
            {
                var ms = new MemoryStream(header.FileSize);
                ds.CopyTo(ms);
                return ms;
            }
        }
    }
}
