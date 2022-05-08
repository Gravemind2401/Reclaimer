using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Halo2
{
    public struct DataPointer
    {
        private readonly ICacheFile cache;
        private readonly int pointer;

        public DataPointer(int pointer, ICacheFile cache)
        {
            this.pointer = pointer;
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public DataPointer(DependencyReader reader, ICacheFile cache)
        {
            pointer = reader?.ReadInt32() ?? throw new ArgumentNullException(nameof(reader));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public int Value => pointer;
        public DataLocation Location => (DataLocation)((Value & 0xC0000000) >> 30);
        public int Address => Value & 0x3FFFFFFF;

        public byte[] ReadData(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var directory = Directory.GetParent(cache.FileName).FullName;

            string target;
            switch (Location)
            {
                case DataLocation.MainMenu:
                    target = Path.Combine(directory, CacheFile.MainMenuMap);
                    break;
                case DataLocation.Shared:
                    target = Path.Combine(directory, CacheFile.SharedMap);
                    break;
                case DataLocation.SinglePlayerShared:
                    target = Path.Combine(directory, CacheFile.SinglePlayerSharedMap);
                    break;
                default:
                    target = cache.FileName;
                    break;
            }

            using (var fs = new FileStream(target, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs))
            {
                reader.Seek(Address, SeekOrigin.Begin);
                return reader.ReadBytes(size);
            }
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(DataPointer value1, DataPointer value2) => value1.pointer == value2.pointer;
        public static bool operator !=(DataPointer value1, DataPointer value2) => !(value1 == value2);

        public static bool Equals(DataPointer value1, DataPointer value2) => value1.pointer.Equals(value2.pointer);
        public override bool Equals(object obj) => obj is DataPointer value && DataPointer.Equals(this, value);
        public bool Equals(DataPointer value) => DataPointer.Equals(this, value);

        public override int GetHashCode() => pointer.GetHashCode();

        #endregion
    }

    public enum DataLocation
    {
        Local = 0,
        MainMenu = 1,
        Shared = 2,
        SinglePlayerShared = 3
    }
}
