using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    internal struct CacheArgs
    {
        //when read using little endian
        internal const int LittleHeader = 0x68656164;
        internal const int BigHeader = 0x64616568;

        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public CacheType CacheType { get; }
        public string BuildString { get; }

        private CacheArgs(string fileName, ByteOrder byteOrder, string buildString, CacheType cacheType)
        {
            FileName = fileName;
            ByteOrder = byteOrder;
            CacheType = cacheType;
            BuildString = buildString;
        }

        public static CacheArgs FromFile(string fileName)
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
                    throw Exceptions.NotAValidMapFile(fileName);

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
                    else throw Exceptions.NotAValidMapFile(fileName);
                }
                else if (reader.ByteOrder == ByteOrder.LittleEndian)
                    buildAddress = 288;
                else buildAddress = 284;

                reader.Seek(buildAddress, SeekOrigin.Begin);
                var buildString = reader.ReadNullTerminatedString(32);
                System.Diagnostics.Debug.WriteLine($"Found build string {buildString ?? "\\0"}");

                var cacheType = CacheType.Unknown;
                foreach (var fi in typeof(CacheType).GetFields().Where(f => f.FieldType == typeof(CacheType)))
                {
                    foreach (BuildStringAttribute attr in fi.GetCustomAttributes(typeof(BuildStringAttribute), false))
                    {
                        if (attr.BuildString != buildString)
                            continue;

                        cacheType = (CacheType)fi.GetValue(null);
                        System.Diagnostics.Debug.WriteLine($"Resolved CacheType {cacheType}");
                        break;
                    }

                    if (cacheType != CacheType.Unknown)
                        break;
                }

                return new CacheArgs(fileName, reader.ByteOrder, buildString, cacheType);
            }
        }
    }
}
