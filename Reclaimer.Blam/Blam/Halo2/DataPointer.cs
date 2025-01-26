using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
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
            Exceptions.ThrowIfNonPositive(size);

            var directory = Directory.GetParent(cache.FileName).FullName;
            var target = Location switch
            {
                _ when cache.Metadata.IsMcc && tag.ClassCode == "bitm" => Path.Combine(directory, CacheFile.MccTextureFile),
                DataLocation.MainMenu => Path.Combine(directory, CacheFile.MainMenuMap),
                DataLocation.Shared => Path.Combine(directory, CacheFile.SharedMap),
                DataLocation.SinglePlayerShared => Path.Combine(directory, CacheFile.SinglePlayerSharedMap),
                _ => cache.FileName,
            };

            //workaround for local bitmaps in workshop maps
            if (cache.Metadata.IsMcc && !File.Exists(target))
                target = cache.FileName;

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

                    //not negative = not compressed
                    if (count == 1 && segments[0] < 0)
                        return reader.ReadBytes(-segments[0]);

                    reader.ReadInt16(); //zlib header?
                }

                return Deflate(fs, segments);
            }
            //h2v has compressed bitmap resources
            else if (cache.CacheType == CacheType.Halo2Vista && tag.ClassCode == "bitm")
            {
                //not sure what the first 2 bytes are, but theyre not part of the stream
                fs.Seek(Address + 2, SeekOrigin.Begin);
                return Deflate(fs, size);
            }
            else
            {
                using (var reader = new EndianReader(fs))
                {
                    reader.Seek(Address, SeekOrigin.Begin);
                    return reader.ReadBytes(size);
                }
            }

            static byte[] Deflate(Stream source, params int[] compressedBlockSizes)
            {
                var origin = source.Position;
                var offset = 0;

                using (var ms = new MemoryStream(compressedBlockSizes.Sum()))
                {
                    foreach (var blockSize in compressedBlockSizes)
                    {
                        source.Seek(origin + offset, SeekOrigin.Begin);
                        using (var ds = new DeflateStream(source, CompressionMode.Decompress, true))
                        {
                            ds.CopyTo(ms);
                            offset += blockSize;
                        }
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
