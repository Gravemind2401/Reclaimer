using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo4
{
    public enum PageType
    {
        Auto,
        Primary,
        Secondary,
        Tertiary
    }

    public struct ResourceIdentifier
    {
        private readonly ICacheFile cache;
        private readonly int identifier; //actually two shorts

        public ResourceIdentifier(int identifier, ICacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            this.identifier = identifier;
        }

        public ResourceIdentifier(DependencyReader reader, ICacheFile cache)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            identifier = reader.ReadInt32();
        }

        public int Value => identifier;

        public int ResourceIndex => identifier & ushort.MaxValue;

        public byte[] ReadData(PageType mode) => ReadData(mode, int.MaxValue);

        public byte[] ReadData(PageType mode, int maxLength)
        {
            var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<cache_file_resource_gestalt>();
            var resourceLayoutTable = cache.TagIndex.GetGlobalTag("play").ReadMetadata<cache_file_resource_layout_table>();

            var entry = resourceGestalt.ResourceEntries[ResourceIndex];

            if (entry.SegmentIndex < 0)
                throw new InvalidOperationException("Data not found");

            var segment = resourceLayoutTable.Segments[entry.SegmentIndex];
            var useTertiary = mode == PageType.Tertiary || (mode == PageType.Auto && segment.TertiaryPageIndex >= 0);
            var useSecondary = mode == PageType.Secondary || (mode == PageType.Auto && segment.SecondaryPageIndex >= 0);

            var pageIndex = useTertiary ? segment.TertiaryPageIndex : useSecondary ? segment.SecondaryPageIndex : segment.PrimaryPageIndex;
            var segmentOffset = useTertiary ? segment.TertiaryPageOffset : useSecondary ? segment.SecondaryPageOffset : segment.PrimaryPageOffset;

            if (pageIndex < 0 || segmentOffset < 0)
                throw new InvalidOperationException("Data not found");

            var page = resourceLayoutTable.Pages[pageIndex];
            if (mode == PageType.Auto && (page.DataOffset < 0 || page.CompressedSize == 0))
            {
                pageIndex = segment.PrimaryPageIndex;
                segmentOffset = segment.PrimaryPageOffset;
                page = resourceLayoutTable.Pages[pageIndex];
            }

            var targetFile = cache.FileName;
            if (page.CacheIndex >= 0)
            {
                var directory = Directory.GetParent(cache.FileName).FullName;
                var mapName = Utils.GetFileName(resourceLayoutTable.SharedCaches[page.CacheIndex].FileName);
                targetFile = Path.Combine(directory, mapName);
            }

            using (var fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, cache.ByteOrder))
            {
                reader.Seek(cache.CacheType >= CacheType.MccHalo4 ? 1216 : 1152, SeekOrigin.Begin);
                var dataTableAddress = reader.ReadUInt32();
                reader.Seek(dataTableAddress + page.DataOffset, SeekOrigin.Begin);

                var segmentLength = Math.Min(maxLength, page.DecompressedSize - segmentOffset);
                if (page.CompressedSize == page.DecompressedSize)
                {
                    reader.Seek(segmentOffset, SeekOrigin.Current);
                    return reader.ReadBytes(segmentLength);
                }

                if (cache.CacheType <= CacheType.Halo4Beta)
                {
                    using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                    using (var reader2 = new BinaryReader(ds))
                    {
                        reader2.ReadBytes(segmentOffset);
                        return reader2.ReadBytes(segmentLength);
                    }
                }
#if DEBUG
                else if (cache.CacheType > CacheType.Halo4Retail) //experimental
                {
                    using (var ms = new MemoryStream())
                    using (var mw = new BinaryWriter(ms))
                    using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                    using (var reader2 = new BinaryReader(ds))
                    {
                        var dataSize = page.DecompressedSize - chunkOffset;
                        for (int i = 0; i < dataSize;)
                        {
                            bool flag;
                            var blockSize = ReadSpecialInt(reader2, out flag);
                            if (flag) reader2.ReadBytes(2);
                            mw.Write(reader2.ReadBytes(blockSize));
                            i += blockSize;
                        }

                        //File.WriteAllBytes("C:\\dump.bin", ms.ToArray());
                        return ms.ToArray();
                    }
                }
#endif
                else
                {
                    var compressed = reader.ReadBytes(page.CompressedSize);
                    var decompressed = new byte[page.DecompressedSize];

                    int startSize = page.CompressedSize;
                    int endSize = page.DecompressedSize;
                    int decompressionContext = 0;
                    XCompress.XMemCreateDecompressionContext(XCompress.XMemCodecType.LZX, 0, 0, ref decompressionContext);
                    XCompress.XMemResetDecompressionContext(decompressionContext);
                    XCompress.XMemDecompressStream(decompressionContext, decompressed, ref endSize, compressed, ref startSize);
                    XCompress.XMemDestroyDecompressionContext(decompressionContext);

                    if (decompressed.Length == segmentLength)
                        return decompressed;

                    var result = new byte[segmentLength];
                    Array.Copy(decompressed, segmentOffset, result, 0, result.Length);
                    return result;
                }
            }
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        private static int ReadSpecialInt(BinaryReader reader, out bool flag) //basically a variant of 7bit encoded int
        {
            flag = false;
            var isFirst = true;

            var result = 0;
            var shift = 0;

            byte b;
            do
            {
                var bits = isFirst ? 6 : 7;

                var mask = (1 << bits) - 1;

                b = reader.ReadByte();
                result |= (b & mask) << shift;
                shift += bits;

                if (isFirst)
                {
                    flag = (b & 0x40) != 0;
                    isFirst = false;
                }

            } while ((b & 0x80) != 0);

            return result;
        }

        #region Equality Operators

        public static bool operator ==(ResourceIdentifier value1, ResourceIdentifier value2)
        {
            return value1.identifier == value2.identifier;
        }

        public static bool operator !=(ResourceIdentifier value1, ResourceIdentifier value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(ResourceIdentifier value1, ResourceIdentifier value2)
        {
            return value1.identifier.Equals(value2.identifier);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is ResourceIdentifier))
                return false;

            return ResourceIdentifier.Equals(this, (ResourceIdentifier)obj);
        }

        public bool Equals(ResourceIdentifier value)
        {
            return ResourceIdentifier.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return identifier.GetHashCode();
        }

        #endregion
    }
}
