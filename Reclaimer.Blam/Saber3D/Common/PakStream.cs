using Reclaimer.IO;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Reclaimer.Saber3D.Common
{
    public class PakStream : ChunkStream
    {
        public PakStream(string filePath)
            : base(filePath)
        { }

        protected override IList<(int, int, int)> ReadChunks()
        {
            using var reader = new EndianReader(BaseStream, ByteOrder.LittleEndian, true);

            reader.Seek(0, SeekOrigin.Begin);

            var chunkCount = reader.ReadInt32();
            var chunks = new (int, int, int)[chunkCount];

            var offsets = new int[chunkCount + 1];
            offsets[^1] = (int)BaseStream.Length;

            for (var i = 0; i < chunkCount; i++)
                offsets[i] = reader.ReadInt32();

            for (var i = 0; i < chunkCount; i++)
            {
                reader.Seek(offsets[i], SeekOrigin.Begin);
                var dataSize = reader.ReadInt32();
                var chunkAddress = (int)reader.Position;
                var chunkSize = offsets[i + 1] - chunkAddress;

                chunks[i] = (chunkAddress, chunkSize, dataSize);
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

            return chunks;
        }

        protected override Stream GetChunkStream(byte[] chunkData)
        {
            return new ZLibStream(new MemoryStream(chunkData), CompressionMode.Decompress);
        }
    }
}
