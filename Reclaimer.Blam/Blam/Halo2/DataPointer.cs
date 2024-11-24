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
                DataLocation.MainMenu => Path.Combine(directory, CacheFile.MainMenuMap),
                DataLocation.Shared => Path.Combine(directory, CacheFile.SharedMap),
                DataLocation.SinglePlayerShared => Path.Combine(directory, CacheFile.SinglePlayerSharedMap),
                _ => cache.FileName,
            };

            using (var fs = new FileStream(target, FileMode.Open, FileAccess.Read))
            {
                //h2v has compressed bitmap resources
                if (cache.CacheType == CacheType.Halo2Vista && tag.ClassCode == "bitm")
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
            }

            static byte[] Deflate(Stream source, int compressedSize)
            {
                using (var ds = new DeflateStream(source, CompressionMode.Decompress, true))
                using (var ms = new MemoryStream(compressedSize))
                {
                    ds.CopyTo(ms);
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
