using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Globalization;
using System.IO;

namespace Reclaimer.Blam.Halo4
{
    public enum PageType
    {
        Auto,
        Primary,
        Secondary,
        Tertiary
    }

    public readonly record struct ResourceIdentifier
    {
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
                var dataTableAddress = GetDataTableAddress(cache, reader);
                reader.Seek(dataTableAddress + page.DataOffset, SeekOrigin.Begin);
                return ContentFactory.GetResourceData(reader, cache.Metadata.ResourceCodec, maxLength, segmentOffset, page.CompressedSize, page.DecompressedSize);
            }

            static uint GetDataTableAddress(ICacheFile cache, EndianReader reader)
            {
                //latest mcc
                if ((cache.Metadata.Engine == BlamEngine.Halo4 && cache.CacheType >= CacheType.MccHalo4U4)
                    || (cache.Metadata.Engine == BlamEngine.Halo2X && cache.CacheType >= CacheType.MccHalo2XU8))
                {
                    reader.Seek(1224, SeekOrigin.Begin);
                    return reader.ReadUInt32();
                }

                //early mcc
                if (cache.Metadata.IsMcc)
                {
                    reader.Seek(1216, SeekOrigin.Begin);
                    return reader.ReadUInt32();
                }

                //xbox
                if (cache.Metadata.Platform == CachePlatform.Xbox360)
                {
                    reader.Seek(1152, SeekOrigin.Begin);
                    return reader.ReadUInt32();
                }

                throw Exceptions.ResourceDataNotSupported(cache);
            }
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);
    }
}