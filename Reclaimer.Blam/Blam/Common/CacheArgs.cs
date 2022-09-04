using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common
{
    internal class CacheArgs
    {
        private const string BuildStringRegex = @"[A-Za-z0-9\. _:]{10,32}";

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
                    if (x == 0)
                        buildAddress = 288; //Halo2 Xbox
                    else if (x == -1)
                        buildAddress = 300; //Halo2 Vista
                    else
                        throw Exceptions.NotAValidMapFile(fileName);
                }
                //else if (version == 10) //MccHalo2
                //{
                //
                //}
                else if (version == 13)
                {
                    reader.Seek(64, SeekOrigin.Begin);
                    if (reader.PeekInt32() == 0) //test for padding
                        buildAddress = 288; //Gen3 MCC (pre August 2021)
                    else
                    {
                        var test = reader.ReadNullTerminatedString(32);
                        if (Regex.IsMatch(test, BuildStringRegex))
                            buildAddress = 64; //MccHalo1
                        else
                        {
                            reader.Seek(160, SeekOrigin.Begin);
                            if (DateTime.TryParse(reader.ReadNullTerminatedString(32), out _))
                                buildAddress = 160; //Gen3 MCC
                            else
                                buildAddress = 152; //Gen4 MCC (April 2022+)
                        }
                    }
                }
                //else if (version == 343) //MCC H1 Custom
                //{
                //
                //}
                else if (reader.ByteOrder == ByteOrder.LittleEndian)
                    buildAddress = 288; //Gen3 MCC
                else
                    buildAddress = 284; //Gen3 x360

                reader.Seek(buildAddress, SeekOrigin.Begin);
                var buildString = reader.ReadNullTerminatedString(32);
                System.Diagnostics.Debug.WriteLine($"Found build string {buildString ?? "\\0"}");

                return new CacheArgs(fileName, reader.ByteOrder, version, buildString, CacheMetadata.FromBuildString(buildString, fileName));
            }
        }
    }
}
