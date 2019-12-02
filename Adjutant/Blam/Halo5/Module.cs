using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
{
    public class Module
    {
        private const int ModuleHeader = 0x64686f6d;

        public string FileName { get; }

        public ModuleType ModuleType => Header.Version;
        public ModuleHeader Header { get; }

        public DependencyReader CreateReader()
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, ByteOrder.LittleEndian);

            var header = reader.PeekInt32();

            if (header != ModuleHeader)
                throw Exceptions.NotAValidMapFile(FileName);

            reader.RegisterInstance(this);

            return reader;
        }
    }
    
    [FixedSize(48, MaxVersion = (int)ModuleType.Halo5Forge)]
    [FixedSize(56, MinVersion = (int)ModuleType.Halo5Forge)]
    public class ModuleHeader
    {
        [Offset(0)]
        public int Head { get; set; }

        [Offset(4)]
        [VersionNumber]
        public ModuleType Version { get; set; }

        [Offset(8)]
        public long ModuleId { get; set; }

        [Offset(16)]
        public int ItemCount { get; set; }

        [Offset(20)]
        public int ManifestCount { get; set; }

        [Offset(24)]
        public int ResourceIndex { get; set; }

        [Offset(28)]
        public int StringsSize { get; set; }

        [Offset(32)]
        public int ResourceCount { get; set; }

        [Offset(36)]
        public int BlockCount { get; set; }
    }

    [FixedSize(88)]
    public class ModuleItem
    {
        private readonly Module module;

        [Offset(0)]
        public int NameOffset { get; set; }

        [Offset(4)]
        public int ParentIndex { get; set; }

        [Offset(8)]
        public int ResourceCount { get; set; }

        [Offset(12)]
        public int ResourceIndex { get; set; }

        [Offset(16)]
        public int BlockCount { get; set; }

        [Offset(20)]
        public int BlockIndex { get; set; }

        [Offset(24)]
        public long DataOffset { get; set; }

        [Offset(32)]
        [StoreType(typeof(uint))]
        public long TotalCompressedSize { get; set; }

        [Offset(36)]
        [StoreType(typeof(uint))]
        public long TotalUncompressedSize { get; set; }

        [Offset(43)]
        public FileEntryFlags Flags { get; set; }

        [Offset(44)]
        public int GlobalTagId { get; set; }

        [Offset(48)]
        public long AssetId { get; set; }

        [Offset(56)]
        public long AssetChecksum { get; set; }

        [Offset(64)]
        [ByteOrder(ByteOrder.BigEndian)]
        public int ClassId { get; set; }

        [Offset(68)]
        [StoreType(typeof(uint))]
        public long UncompressedHeaderSize { get; set; }

        [Offset(72)]
        [StoreType(typeof(uint))]
        public long UncompressedTagDataSize { get; set; }

        [Offset(76)]
        [StoreType(typeof(uint))]
        public long UncompressedResourceDataSize { get; set; }

        [Offset(80)]
        public short HeaderBlockCount { get; set; }

        [Offset(82)]
        public short TagDataBlockCount { get; set; }

        [Offset(84)]
        public short ResourceBlockCount { get; set; }

        public string ClassCode => (ClassId == -1) ? null : Encoding.ASCII.GetString(BitConverter.GetBytes(ClassId));

        public ModuleItem(Module module)
        {
            this.module = module;
        }
    }

    [FixedSize(20, MaxVersion = (int)ModuleType.Halo5Forge)]
    [FixedSize(32, MinVersion = (int)ModuleType.Halo5Forge)]
    public class Block
    {
        [StoreType(typeof(uint))]
        [Offset(0, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(8, MinVersion = (int)ModuleType.Halo5Forge)]
        public long CompressedOffset { get; set; }

        [StoreType(typeof(uint))]
        [Offset(4, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(12, MinVersion = (int)ModuleType.Halo5Forge)]
        public long CompressedSize { get; set; }

        [StoreType(typeof(uint))]
        [Offset(8, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(16, MinVersion = (int)ModuleType.Halo5Forge)]
        public long UncompressedOffset { get; set; }

        [StoreType(typeof(uint))]
        [Offset(12, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(20, MinVersion = (int)ModuleType.Halo5Forge)]
        public long UncompressedSize { get; set; }

        [Offset(16, MaxVersion = (int)ModuleType.Halo5Forge)]
        [Offset(24, MinVersion = (int)ModuleType.Halo5Forge)]
        public int Compressed { get; set; }
    }
}
