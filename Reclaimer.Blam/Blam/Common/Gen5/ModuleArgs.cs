using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.Common.Gen5
{
    internal class ModuleArgs
    {
        internal const int ModuleHeader = 0x64686f6d;

        public string FileName { get; }
        public ModuleType Version { get; }

        public bool UsesOodle => Version == ModuleType.HaloInfinite;
        public bool UsesStringMap => Version == ModuleType.HaloInfinite;

        private ModuleArgs(string fileName, ModuleType version)
        {
            FileName = fileName;
            Version = version;
        }

        public static ModuleArgs FromFile(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new EndianReader(fs, ByteOrder.LittleEndian))
            {
                var header = reader.ReadInt32();
                if (header != ModuleHeader)
                    throw Exceptions.NotAValidModuleFile(fileName);

                var version = reader.ReadInt32();
                return new ModuleArgs(fileName, (ModuleType)version);
            }
        }
    }
}
