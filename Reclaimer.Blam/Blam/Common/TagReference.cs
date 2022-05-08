using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    [FixedSize(16, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(8, MinVersion = (int)CacheType.Halo2Xbox, MaxVersion = (int)CacheType.Halo3Beta)]
    [FixedSize(16, MinVersion = (int)CacheType.Halo3Beta)]
    public struct TagReference : IWriteable
    {
        public static TagReference NullReference { get; } = new TagReference(null, -1, -1);

        private readonly ICacheFile cache;
        private readonly int classId;
        private readonly int tagId;

        public int TagId => (short)(tagId & ushort.MaxValue);
        public IIndexItem Tag => TagId >= 0 ? cache?.TagIndex[TagId] : null;

        public TagReference(ICacheFile cache, int classId, int tagId)
        {
            this.cache = cache;
            this.classId = classId;
            this.tagId = tagId;
        }

        public TagReference(ICacheFile cache, EndianReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            classId = reader.ReadInt32();

            if (cache.Metadata.Generation != CacheGeneration.Gen2)
                reader.Seek(8, SeekOrigin.Current);
            else if (cache.CacheType == CacheType.Halo2Beta)
                reader.Seek(8, SeekOrigin.Current);

            tagId = reader.ReadInt32();
        }

        public void Write(EndianWriter writer) => Write(writer, null);

        public void Write(EndianWriter writer, double? version)
        {
            writer.Write(classId);

            if (cache.Metadata.Generation != CacheGeneration.Gen2)
            {
                writer.Write(0);
                writer.Write(0);
            }
            else if (cache.CacheType == CacheType.Halo2Beta)
            {
                writer.Write(0);
                writer.Write(0);
            }

            writer.Write(tagId);
        }

        public override string ToString() => Tag?.ToString();

        #region Equality Operators

        public static bool operator ==(TagReference value1, TagReference value2) => value1.cache != null && value2.cache != null && value1.cache == value2.cache && value1.tagId == value2.tagId;
        public static bool operator !=(TagReference value1, TagReference value2) => !(value1 == value2);

        public static bool Equals(TagReference value1, TagReference value2) => value1.cache != null && value2.cache != null && value1.cache.Equals(value2.cache) && value1.tagId.Equals(value2.tagId);
        public override bool Equals(object obj) => obj is TagReference value && TagReference.Equals(this, value);
        public bool Equals(TagReference value) => TagReference.Equals(this, value);

        public override int GetHashCode() => cache?.GetHashCode() ?? 0 ^ tagId.GetHashCode();

        #endregion
    }
}
