using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Globalization;
using System.IO;

namespace Reclaimer.Blam.Halo2
{
    public readonly record struct DataPointer
    {
        private readonly ICacheFile cache;

        public int Value { get; }

        public DataPointer(int pointer, ICacheFile cache)
        {
            Value = pointer;
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public DataPointer(DependencyReader reader, ICacheFile cache)
        {
            Value = reader?.ReadInt32() ?? throw new ArgumentNullException(nameof(reader));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public DataLocation Location => (DataLocation)((Value & 0xC0000000) >> 30);
        public int Address => Value & 0x3FFFFFFF;

        public byte[] ReadData(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var directory = Directory.GetParent(cache.FileName).FullName;
            var target = Location switch
            {
                DataLocation.MainMenu => Path.Combine(directory, CacheFile.MainMenuMap),
                DataLocation.Shared => Path.Combine(directory, CacheFile.SharedMap),
                DataLocation.SinglePlayerShared => Path.Combine(directory, CacheFile.SinglePlayerSharedMap),
                _ => cache.FileName,
            };

            using (var fs = new FileStream(target, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs))
            {
                reader.Seek(Address, SeekOrigin.Begin);
                return reader.ReadBytes(size);
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
