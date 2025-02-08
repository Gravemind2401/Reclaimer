using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Globalization;
using System.IO;

namespace Reclaimer.Blam.Halo3
{
    public enum PageType
    {
        Auto,
        Primary,
        Secondary
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly record struct ResourceIdentifier
    {
        private const string shared_map = "shared.map";

        private readonly ICacheFile cache;

        public ResourceIdentifier(int identifier, ICacheFile cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Value = identifier;
        }

        public ResourceIdentifier(DependencyReader reader, ICacheFile cache)
        {
            ArgumentNullException.ThrowIfNull(reader);
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            Value = reader.ReadInt32();
        }

        public int Value { get; } //actually two shorts
        public int ResourceIndex => Value & ushort.MaxValue;

        public byte[] ReadData(PageType mode) => ReadData(mode, int.MaxValue);

        public byte[] ReadData(PageType mode, int maxLength)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

            if (cache.CacheType <= CacheType.Halo3Beta)
                return ReadDataHalo3Beta(mode, maxLength);

            var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<CacheFileResourceGestaltTag>();
            var resourceLayoutTable = cache.TagIndex.GetGlobalTag("play").ReadMetadata<CacheFileResourceLayoutTableTag>();
            var entry = resourceGestalt.ResourceEntries[ResourceIndex];

            if (entry.SegmentIndex < 0)
                throw new InvalidOperationException("Data not found");

            var segment = resourceLayoutTable.Segments[entry.SegmentIndex];
            var useSecondary = mode == PageType.Secondary || (mode == PageType.Auto && segment.SecondaryPageIndex >= 0);

            var pageIndex = useSecondary ? segment.SecondaryPageIndex : segment.PrimaryPageIndex;
            var segmentOffset = useSecondary ? segment.SecondaryPageOffset : segment.PrimaryPageOffset;

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
                var dataTableAddress = GetDataTableAddress(cache, reader);
                reader.Seek(dataTableAddress + page.DataOffset, SeekOrigin.Begin);
                return ContentFactory.GetResourceData(reader, cache.Metadata.ResourceCodec, maxLength, segmentOffset, page.CompressedSize, page.DecompressedSize);
            }

            uint GetDataTableAddress(ICacheFile cache, EndianReader reader)
            {
                //latest mcc
                if ((cache.Metadata.Engine == BlamEngine.Halo3 && cache.CacheType >= CacheType.MccHalo3U6)
                    || (cache.Metadata.Engine == BlamEngine.Halo3ODST && cache.CacheType >= CacheType.MccHalo3ODSTU3))
                {
                    if (page.CacheIndex >= 0)
                        return 16384; //header size
                    else
                    {
                        reader.Seek(1232, SeekOrigin.Begin);
                        return reader.ReadUInt32();
                    }
                }

                //mcc - H3 U6+, ODST U3+
                if ((cache.Metadata.Engine == BlamEngine.Halo3 && cache.CacheType >= CacheType.MccHalo3F6)
                    || (cache.Metadata.Engine == BlamEngine.Halo3ODST && cache.CacheType >= CacheType.MccHalo3ODSTF3))
                {
                    if (page.CacheIndex >= 0)
                        return 16384; //header size
                    else
                    {
                        reader.Seek(1200, SeekOrigin.Begin);
                        return reader.ReadUInt32();
                    }
                }

                //early mcc
                if (cache.Metadata.IsMcc)
                {
                    if (page.CacheIndex >= 0)
                        return 12288; //header size
                    else
                    {
                        reader.Seek(1208, SeekOrigin.Begin);
                        return reader.ReadUInt32();
                    }
                }

                //xbox
                if (cache.Metadata.Platform == CachePlatform.Xbox360)
                {
                    reader.Seek(1136, SeekOrigin.Begin);
                    return reader.ReadUInt32();
                }

                throw Exceptions.ResourceDataNotSupported(cache);
            }
        }

        private byte[] ReadDataHalo3Beta(PageType mode, int maxLength)
        {
            var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<CacheFileResourceGestaltTag>();
            var directory = Directory.GetParent(cache.FileName).FullName;
            var entry = resourceGestalt.ResourceEntries[ResourceIndex];

            var useSecondary = mode == PageType.Secondary || (mode == PageType.Auto && entry.SecondaryOffset > 0);

            var address = useSecondary ? entry.SecondaryOffset : entry.PrimaryOffset;
            var size = useSecondary ? entry.SecondarySize : entry.PrimarySize;

            var targetFile = entry.CacheIndex == -1 || cache.CacheType < CacheType.Halo3Delta
                ? cache.FileName
                : Path.Combine(directory, shared_map);

            using (var fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs))
            {
                reader.Seek(address, SeekOrigin.Begin);
                return ContentFactory.GetResourceData(reader, cache.Metadata.ResourceCodec, maxLength, 0, size, size);
            }
        }

        public byte[] ReadSoundData()
        {
            var directory = Directory.GetParent(cache.FileName).FullName;
            var resourceGestalt = cache.TagIndex.GetGlobalTag("zone").ReadMetadata<CacheFileResourceGestaltTag>();
            var resourceLayoutTable = cache.TagIndex.GetGlobalTag("play").ReadMetadata<CacheFileResourceLayoutTableTag>();
            var entry = resourceGestalt.ResourceEntries[ResourceIndex];

            if (entry.SegmentIndex < 0)
                throw new InvalidOperationException("Data not found");

            var segment = resourceLayoutTable.Segments[entry.SegmentIndex];
            var sizeGroup1 = resourceLayoutTable.SizeGroups[segment.PrimarySizeIndex];
            var sizeGroup2 = resourceLayoutTable.SizeGroups[segment.SecondarySizeIndex];
            var page1 = resourceLayoutTable.Pages[segment.PrimaryPageIndex];
            var page2 = resourceLayoutTable.Pages[segment.SecondaryPageIndex];

            if (page1.CompressedSize != page1.DecompressedSize || page2.CompressedSize != page2.DecompressedSize)
                throw new NotSupportedException("Compressed sound data");

            var output = new byte[sizeGroup1.TotalSize + sizeGroup2.TotalSize];

            if (page1.CompressedSize > 0 && sizeGroup1.TotalSize > 0)
            {
                var pageData = ReadSoundData(directory, resourceLayoutTable, page1, sizeGroup1.TotalSize);
                var pageOffset = segment.PrimaryPageOffset;

                foreach (var size in sizeGroup1.Sizes)
                {
                    Array.Copy(pageData, pageOffset, output, size.Offset, size.DataSize);
                    pageOffset += size.DataSize;
                }
            }

            if (page2.CompressedSize > 0 && sizeGroup2.TotalSize > 0)
            {
                var pageData = ReadSoundData(directory, resourceLayoutTable, page2, sizeGroup2.TotalSize);
                var pageOffset = segment.SecondaryPageOffset;

                foreach (var size in sizeGroup2.Sizes)
                {
                    Array.Copy(pageData, pageOffset, output, size.Offset, size.DataSize);
                    pageOffset += size.DataSize;
                }
            }

            return output;
        }

        private byte[] ReadSoundData(string directory, CacheFileResourceLayoutTableTag resourceLayoutTable, PageBlock page, int size)
        {
            var targetFile = cache.FileName;
            if (page.CacheIndex >= 0)
            {
                var mapName = Utils.GetFileName(resourceLayoutTable.SharedCaches[page.CacheIndex].FileName);
                targetFile = Path.Combine(directory, mapName);
            }

            using (var fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, cache.ByteOrder))
            {
                reader.Seek(1136, SeekOrigin.Begin);
                var dataTableAddress = reader.ReadInt32();

                reader.Seek(dataTableAddress + page.DataOffset, SeekOrigin.Begin);
                return reader.ReadBytes(Math.Max(page.CompressedSize, size));
            }
        }

        private string GetDebuggerDisplay() => Value.ToString(CultureInfo.CurrentCulture);
    }
}
