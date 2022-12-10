using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;

namespace Reclaimer.Blam.Common
{
    public readonly record struct StringId : IWriteable
    {
        private readonly ICacheFile cache;

        public int Id { get; }

        public StringId(int id, ICacheFile cache)
        {
            Id = id;
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public StringId(EndianReader reader, ICacheFile cache)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            Id = cache.CacheType < CacheType.Halo3Alpha ? reader.ReadInt16() : reader.ReadInt32();
            this.cache = cache;
        }

        public string Value
        {
            get
            {
                try { return cache?.StringIndex?[Id]; }
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

        public static implicit operator string(StringId value) => value.Value;
        public static explicit operator int(StringId value) => value.Id;
    }
}
