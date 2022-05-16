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
            this.id = id;
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
            else
                writer.Write(Id);
        }

        public override string ToString() => Value;

        #region Equality Operators

        public static bool operator ==(StringId value1, StringId value2) => value1.id == value2.id;
        public static bool operator !=(StringId value1, StringId value2) => !(value1 == value2);

        public static bool Equals(StringId value1, StringId value2) => value1.id.Equals(value2.id);
        public override bool Equals(object obj) => obj is StringId value && StringId.Equals(this, value);
        public bool Equals(StringId value) => StringId.Equals(this, value);

        public override int GetHashCode() => id.GetHashCode();

        #endregion

        public static implicit operator string(StringId value) => value.Value;
        public static explicit operator int(StringId value) => value.id;
    }
}
