using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    public struct StringId : IWriteable
    {
        private readonly int id;
        private readonly ICacheFile cache;

        public StringId(int id, ICacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.id = id;
            this.cache = cache;
        }

        public StringId(EndianReader reader, ICacheFile cache)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            id = cache.CacheType < CacheType.Halo3Alpha ? reader.ReadInt16() : reader.ReadInt32();
            this.cache = cache;
        }

        public int Id => id;
        public string Value
        {
            get
            {
                try { return cache?.StringIndex?[id]; }
                catch { return "<invalid>"; }
            }
        }

        public void Write(EndianWriter writer, double? version)
        {
            if (cache.CacheType < CacheType.Halo3Alpha)
                writer.Write((short)Id);
            else writer.Write(Id);
        }

        public override string ToString() => Value;

        #region Equality Operators

        public static bool operator ==(StringId stringId1, StringId stringId2)
        {
            return stringId1.id == stringId2.id;
        }

        public static bool operator !=(StringId stringId1, StringId stringId2)
        {
            return !(stringId1 == stringId2);
        }

        public static bool Equals(StringId stringId1, StringId stringId2)
        {
            return stringId1.id.Equals(stringId2.id);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is StringId))
                return false;

            return StringId.Equals(this, (StringId)obj);
        }

        public bool Equals(StringId value)
        {
            return StringId.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        #endregion

        public static implicit operator string(StringId stringId)
        {
            return stringId.Value;
        }

        public static explicit operator int(StringId stringId)
        {
            return stringId.id;
        }
    }
}
