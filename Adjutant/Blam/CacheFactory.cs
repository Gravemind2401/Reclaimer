using Adjutant.Blam.Definitions;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam
{
    public static class CacheFactory
    {
        internal const int LittleHeader = 0x68656164;
        internal const int BigHeader = 0x64616568;

        public static ICacheFile ReadCacheFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            switch (GetCacheTypeByFile(fileName))
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                case CacheType.Halo1AE:
                    return new Halo1.CacheFile(fileName);

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    return new Halo2.CacheFile(fileName);

                default: throw Exceptions.NotAValidMapFile(Path.GetFileName(fileName));
            }
        }

        public static CacheType GetCacheTypeByFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, ByteOrder.LittleEndian))
            {
                var header = reader.ReadInt32();
                if (header == BigHeader)
                    reader.ByteOrder = ByteOrder.BigEndian;
                else if (header != LittleHeader)
                    throw Exceptions.NotAValidMapFile(Path.GetFileName(fileName));

                var version = reader.ReadInt32();

                int buildAddress;
                if (new[] { 5, 7, 609 }.Contains(version)) // Halo1 Xbox, PC, CE
                    buildAddress = 64;
                else if (version == 8)
                {
                    reader.Seek(36, SeekOrigin.Begin);
                    version = reader.ReadInt32();
                    if (version == 0) buildAddress = 288; //Halo2 Xbox
                    else if (version == -1) buildAddress = 300; //Halo2 Vista
                    else throw Exceptions.NotAValidMapFile(Path.GetFileName(fileName));
                }
                else buildAddress = 284;

                reader.Seek(buildAddress, SeekOrigin.Begin);
                var buildString = reader.ReadNullTerminatedString(32);

                return GetCacheTypeByBuild(buildString);
            }
        }

        public static CacheType GetCacheTypeByBuild(string buildString)
        {
            if (buildString == null)
                throw new ArgumentNullException(nameof(buildString));

            foreach (var fi in typeof(CacheType).GetFields().Where(f => f.FieldType == typeof(CacheType)))
            {
                foreach (BuildStringAttribute attr in fi.GetCustomAttributes(typeof(BuildStringAttribute), false))
                {
                    if (attr.BuildString == buildString)
                        return (CacheType)fi.GetValue(null);
                }
            }

            return CacheType.Unknown;
        }
    }
}
