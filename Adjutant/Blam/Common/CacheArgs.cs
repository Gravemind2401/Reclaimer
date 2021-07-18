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
    internal class CacheArgs
    {
        //when read using little endian
        internal const int LittleHeader = 0x68656164;
        internal const int BigHeader = 0x64616568;

        public string FileName { get; }
        public ByteOrder ByteOrder { get; }
        public int Version { get; }
        public string BuildString { get; }
        public CacheMetadata Metadata { get; }

        public CacheType CacheType => Metadata?.CacheType ?? CacheType.Unknown;

        private CacheArgs(string fileName, ByteOrder byteOrder, int version, string buildString, CacheMetadata metadata)
        {
            FileName = fileName;
            ByteOrder = byteOrder;
            Version = version;
            BuildString = buildString;
            Metadata = metadata;
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
                if (new[] { 5, 6, 7, 609 }.Contains(version)) // Halo1 Xbox, PC, CE
                    buildAddress = 64;
                else if (version == 8)
                {
                    reader.Seek(36, SeekOrigin.Begin);
                    var x = reader.ReadInt32();
                    if (x == 0) buildAddress = 288; //Halo2 Xbox
                    else if (x == -1) buildAddress = 300; //Halo2 Vista
                    else throw Exceptions.NotAValidMapFile(fileName);
                }
                else if (version == 13)
                {
                    reader.Seek(64, SeekOrigin.Begin);
                    buildAddress = reader.ReadInt32() == 0
                        ? 288 //Gen3 MCC
                        : 64; //MccHalo1
                }
                else if (reader.ByteOrder == ByteOrder.LittleEndian)
                    buildAddress = 288; //Gen3 MCC
                else buildAddress = 284; //Gen3 x360

                reader.Seek(buildAddress, SeekOrigin.Begin);
                var buildString = reader.ReadNullTerminatedString(32);
                System.Diagnostics.Debug.WriteLine($"Found build string {buildString ?? "\\0"}");

                return new CacheArgs(fileName, reader.ByteOrder, version, buildString, CacheMetadata.FromBuildString(buildString));
            }
        }
    }
}
