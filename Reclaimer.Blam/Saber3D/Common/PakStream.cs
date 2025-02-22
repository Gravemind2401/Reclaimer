using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace Reclaimer.Saber3D.Common
{
    public class PakStream : ChunkStream
    {
        private static readonly ConditionalWeakTable<IPakFile, Tuple<bool, ChunkLocator[]>> chunkTableCache = new();

        private readonly IPakFile pak;

        internal bool IsX360 { get; private set; }

        public PakStream(IPakFile pakFile)
            : base(pakFile.FileName)
        {
            pak = pakFile;
            InitializeChunks();
        }

        protected override IList<ChunkLocator> ReadChunks()
        {
            if (chunkTableCache.TryGetValue(pak, out var cached))
            {
                IsX360 = cached.Item1;
                return cached.Item2;
            }

            using var reader = new EndianReader(BaseStream, ByteOrder.LittleEndian, true);

            reader.Seek(0, SeekOrigin.Begin);

            var chunkCount = reader.ReadInt32();
            var chunks = new ChunkLocator[chunkCount];

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

                chunks[i] = new ChunkLocator(chunkAddress, chunkSize, dataSize);
            }

            //make corrections for x360 files
            if (chunks.Any(c => c.UncompressedSize < 0 || c.UncompressedSize > 0x007FFFFF))
            {
                IsX360 = true;

                //uncompressed size appears to be max of 32768, compressed size can sometimes be slightly bigger
                const int bufferSize = 0x10000;

                var inBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                var outBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                for (var i = 0; i < chunkCount; i++)
                {
                    var c = chunks[i];
                    c.CompressedSize += sizeof(int);
                    c.SourceAddress -= sizeof(int);

                    reader.Seek(c.SourceAddress, SeekOrigin.Begin);
                    reader.Read(inBuffer, 0, c.CompressedSize.Value);

                    var uncompressedSize = bufferSize;
                    XCompress.DecompressLZX(inBuffer, outBuffer, ref uncompressedSize);

                    c.UncompressedSize = uncompressedSize;
                    chunks[i] = c;
                }

                ArrayPool<byte>.Shared.Return(inBuffer);
                ArrayPool<byte>.Shared.Return(outBuffer);
            }

            chunkTableCache.Add(pak, Tuple.Create(IsX360, chunks));

            return chunks;
        }

        protected override Stream CreateDecompressionStream(Stream sourceStream, bool leaveOpen, int? compressedSize, int uncompressedSize)
        {
            if (!IsX360)
                return new ZLibStream(sourceStream, CompressionMode.Decompress, leaveOpen);

            var data = new byte[compressedSize.Value];
            sourceStream.ReadExactly(data, 0, data.Length);
            data = XCompress.DecompressLZX(data, ref uncompressedSize);
            return new MemoryStream(data);
        }
    }
}
