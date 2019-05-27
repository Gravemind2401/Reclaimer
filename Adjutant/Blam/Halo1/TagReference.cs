using Adjutant.IO;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    [FixedSize(4)]
    public struct TagReference
    {
        private readonly CacheFile cache;
        private readonly short tagId;

        public int TagId => tagId;
        public IndexItem Tag => TagId >= 0 ? cache.TagIndex[TagId] : null;

        public TagReference(CacheFile cache, DependencyReader reader)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            this.cache = cache;
            tagId = reader.ReadInt16();
        }

        public override string ToString() => Tag?.ToString();

        #region Equality Operators

        public static bool operator ==(TagReference value1, TagReference value2)
        {
            return value1.cache != null && value2.cache != null && value1.cache == value2.cache && value1.tagId == value2.tagId;
        }

        public static bool operator !=(TagReference value1, TagReference value2)
        {
            return !(value1 == value2);
        }

        public static bool Equals(TagReference value1, TagReference value2)
        {
            return value1.cache != null && value2.cache != null && value1.cache.Equals(value2.cache) && value1.tagId.Equals(value2.tagId);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is TagReference))
                return false;

            return TagReference.Equals(this, (TagReference)obj);
        }

        public bool Equals(TagReference value)
        {
            return TagReference.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return cache?.GetHashCode() ?? 0 ^ tagId.GetHashCode();
        }

        #endregion
    }
}
