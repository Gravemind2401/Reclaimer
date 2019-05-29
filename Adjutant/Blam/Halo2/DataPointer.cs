using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public struct DataPointer
    {
        private const string mainMenu_map = "mainmenu.map";
        private const string shared_map = "shared.map";
        private const string spShared_map = "single_player_shared.map";

        private readonly CacheFile _cache;
        private readonly int _value;

        public DataPointer(int value, CacheFile cache)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            _value = value;
            _cache = cache;
        }

        public DataPointer(DependencyReader reader, CacheFile cache)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            _value = reader.ReadInt32();
            _cache = cache;
        }

        public int Value => _value;

        public DataLocation Location => (DataLocation)((Value & 0xC0000000) >> 30);

        public int Address => Value & 0x3FFFFFFF;

        public byte[] ReadData(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var directory = Directory.GetParent(_cache.FileName).FullName;

            string target;
            switch (Location)
            {
                case DataLocation.MainMenu:
                    target = Path.Combine(directory, mainMenu_map);
                    break;
                case DataLocation.Shared:
                    target = Path.Combine(directory, shared_map);
                    break;
                case DataLocation.SinglePlayerShared:
                    target = Path.Combine(directory, spShared_map);
                    break;
                default:
                    target = _cache.FileName;
                    break;
            }

            using(var fs = new FileStream(target, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs))
            {
                reader.Seek(Address, SeekOrigin.Begin);
                return reader.ReadBytes(size);
            }
        }

        public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);

        #region Equality Operators

        public static bool operator ==(DataPointer pointer1, DataPointer pointer2)
        {
            return pointer1._value == pointer2._value;
        }

        public static bool operator !=(DataPointer pointer1, DataPointer pointer2)
        {
            return !(pointer1 == pointer2);
        }

        public static bool Equals(DataPointer pointer1, DataPointer pointer2)
        {
            return pointer1._value.Equals(pointer2._value);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is DataPointer))
                return false;

            return DataPointer.Equals(this, (DataPointer)obj);
        }

        public bool Equals(DataPointer value)
        {
            return DataPointer.Equals(this, value);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

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
