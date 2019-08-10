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

namespace Adjutant.Blam.HaloReach
{
    public struct ResourceIdentifier
    {
        private readonly CacheFile cache;
        private readonly int identifier; //actually two shorts

        public ResourceIdentifier(int identifier, CacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.cache = cache;
            this.identifier = identifier;
        }

        public ResourceIdentifier(DependencyReader reader, CacheFile cache)
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

        public byte[] ReadData()
        {
            var directory = Directory.GetParent(cache.FileName).FullName;
            var entry = cache.ResourceGestalt.ResourceEntries[ResourceIndex];

            if (entry.SegmentIndex < 0)
                throw new InvalidOperationException("Data not found");

            var segment = cache.ResourceLayoutTable.Segments[entry.SegmentIndex];
            var pageIndex = segment.OptionalPageIndex >= 0 ? segment.OptionalPageIndex : segment.RequiredPageIndex;
            var chunkOffset = segment.OptionalPageOffset >= 0 ? segment.OptionalPageOffset : segment.RequiredPageOffset;

            if (pageIndex < 0 || chunkOffset < 0)
                throw new InvalidOperationException("Data not found");

            var page = cache.ResourceLayoutTable.Pages[pageIndex];
            if (page.DataOffset < 0)
            {
                pageIndex = segment.RequiredPageIndex;
                chunkOffset = segment.RequiredPageOffset;
                page = cache.ResourceLayoutTable.Pages[pageIndex];
            }

            var targetFile = cache.FileName;
            if (page.CacheIndex >= 0)
            {
                var mapName = Utils.GetFileName(cache.ResourceLayoutTable.SharedCaches[page.CacheIndex].FileName);
                targetFile = Path.Combine(directory, mapName);
            }

            byte[] compressed, decompressed;

            using (var fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, ByteOrder.BigEndian))
            {
                reader.Seek(1136, SeekOrigin.Begin);
                var dataTableAddress = reader.ReadInt32();

                reader.Seek(dataTableAddress + page.DataOffset, SeekOrigin.Begin);
                compressed = reader.ReadBytes(page.CompressedSize);
            }

            using (var ms = new MemoryStream(compressed))
            using (var stream = new DeflateStream(ms, CompressionMode.Decompress))
            using (var br = new BinaryReader(stream))
                decompressed = br.ReadBytes(page.DecompressedSize);

            return decompressed.Skip(chunkOffset).ToArray();
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

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
