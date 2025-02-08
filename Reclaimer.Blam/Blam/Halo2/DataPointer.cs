using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace Reclaimer.Blam.Halo2
{
    public readonly record struct DataPointer
    {
        private readonly IIndexItem tag;
        private ICacheFile cache => tag.CacheFile;

        public int Value { get; }

        public DataPointer(int pointer, IIndexItem tag)
        {
            Value = pointer;
            this.tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        public DataPointer(DependencyReader reader, IIndexItem tag)
        {
            Value = reader?.ReadInt32() ?? throw new ArgumentNullException(nameof(reader));
            this.tag = tag ?? throw new ArgumentNullException(nameof(tag));
        }

        public DataLocation Location => (DataLocation)((Value & 0xC0000000) >> 30);
        public int Address => Value & 0x3FFFFFFF;

        public byte[] ReadData(int size)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

            var directory = Directory.GetParent(cache.FileName).FullName;
            var target = Location switch
            {
                _ when cache.Metadata.IsMcc && tag.ClassCode == "bitm" => Path.Combine(directory, CacheFile.MccTextureFile),
                DataLocation.MainMenu => Path.Combine(directory, CacheFile.MainMenuMap),
                DataLocation.Shared => Path.Combine(directory, CacheFile.SharedMap),
                DataLocation.SinglePlayerShared => Path.Combine(directory, CacheFile.SinglePlayerSharedMap),
                _ => cache.FileName,
            };

            //workaround for workshop maps - bitmaps may be in a texture file or may be local
            if (cache.Metadata.IsMcc && !File.Exists(target))
            {
                target = Path.Combine(directory, @"..\halo2\h2_maps_win64_dx11", CacheFile.MccTextureFile);
                if (!File.Exists(target))
                    target = cache.FileName;
            }

            using var fs = target == cache.FileName
                ? cache.CreateStream()
                : new FileStream(target, FileMode.Open, FileAccess.Read);

            //mcc has bitmap resources split into one or more compressed chunks
            if (cache.Metadata.IsMcc && tag.ClassCode == "bitm")
            {
                fs.Seek(Address, SeekOrigin.Begin);

                int[] segments;
                using (var reader = new EndianReader(fs, cache.ByteOrder, true))
                {
                    var count = reader.ReadInt32();
                    segments = reader.ReadArray<int>(count);
                }

                return DecompressBlocks(fs, segments);
            }
            //h2v has compressed bitmap resources
            else if (cache.CacheType == CacheType.Halo2Vista && tag.ClassCode == "bitm")
            {
                fs.Seek(Address, SeekOrigin.Begin);
                return DecompressBlocks(fs, size);
            }
            else
            {
                using (var reader = new EndianReader(fs))
                {
                    reader.Seek(Address, SeekOrigin.Begin);
                    return reader.ReadBytes(size);
                }
            }

            static byte[] DecompressBlocks(Stream source, params int[] compressedBlockSizes)
            {
                var origin = source.Position;
                var offset = 0;

                using (var ms = new MemoryStream(compressedBlockSizes.Sum(Math.Abs)))
                {
                    foreach (var blockSize in compressedBlockSizes)
                    {
                        source.Seek(origin + offset, SeekOrigin.Begin);
                        if (blockSize < 0) //negative = not compressed
                        {
                            var actualSize = -blockSize;
                            var buffer = ArrayPool<byte>.Shared.Rent(actualSize);
                            source.ReadExactly(buffer, 0, actualSize);
                            ms.Write(buffer, 0, actualSize);
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                        else
                        {
                            using (var ds = new ZLibStream(source, CompressionMode.Decompress, true))
                                ds.CopyTo(ms);
                        }
                        offset += Math.Abs(blockSize);
                    }
                    return ms.ToArray();
                }
            }
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);
    }

    public enum DataLocation
    {
        Local = 0,
        MainMenu = 1,
        Shared = 2,
        SinglePlayerShared = 3
    }
}
