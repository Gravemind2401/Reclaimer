using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.IO;
using System.Text;

namespace Reclaimer.Blam.HaloInfinite
{
    public enum DataOffsetFlags : byte
    {
        UseHD1 = 0b00000001,
        UseHD2 = 0b00000010,
    }

    [FixedSize(88)]
    public class ModuleItem : IModuleItem
    {
        public Module Module { get; }

        [Offset(0)]
        public byte Unk_0x00 { get; set; }

        [Offset(1)]
        public FileEntryFlags Flags { get; set; }

        [Offset(2)]
        public ushort BlockCount { get; set; }

        [Offset(4)]
        public int BlockIndex { get; set; }

        [Offset(8)]
        public int ResourceIndex { get; set; }

        [Offset(12)]
        [ByteOrder(ByteOrder.BigEndian)]
        public int ClassId { get; set; }

        [Offset(16)]
        public long DataOffsetTemp { get; set; }

        [Offset(24)]
        public int TotalCompressedSize { get; set; }

        [Offset(28)]
        public int TotalUncompressedSize { get; set; }

        [Offset(32)]
        public int GlobalTagId { get; set; }

        [Offset(36)]
        public uint UncompressedHeaderSize { get; set; }

        [Offset(40)]
        public uint UncompressedTagSize { get; set; }

        [Offset(44)]
        public uint UncompressedResourceDataSize { get; set; }

        [Offset(48)]
        public uint UncompressedActualResourceSize { get; set; }

        [Offset(52)]
        public byte HeaderAlignment { get; set; }

        [Offset(53)]
        public byte TagDataAlignment { get; set; }

        [Offset(54)]
        public byte ResourceDataAlignment { get; set; }

        [Offset(55)]
        public byte ActualResourceDataAligment { get; set; }

        [Offset(56)]
        public uint NameOffset { get; set; }

        [Offset(60)]
        public int ParentIndex { get; set; }

        [Offset(64)]
        public long AssetChecksum { get; set; }

        [Offset(72)]
        public long AssetId { get; set; }

        [Offset(80)]
        public int ResourceCount { get; set; }

        public string ClassCode => (ClassId == -1) ? "resource" : Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId));

        public long DataOffset => DataOffsetTemp & 0x0000FFFFFFFFFFFF;
        public DataOffsetFlags DataOffsetFlags => (DataOffsetFlags)(DataOffsetTemp >> 48);

        private string tagName;
        public string TagName
        {
            get
            {
                if (tagName != null)
                    return tagName;

                if (GlobalTagId != -1)
                    tagName = StringMapper.Instance.StringMappings.TryGetValue(GlobalTagId, out var value) ? value : GlobalTagId.ToString();
                else
                {
                    var parent = Module.Items[ParentIndex];
                    var childIndex = Module.Resources.Skip(parent.ResourceIndex).Take(parent.ResourceCount)
                        .Select(i => Module.Items[i])
                        .TakeWhile(i => i != this)
                        .Count();

                    tagName = $"{parent.TagName}.{parent.ClassName}[{childIndex}:resource]";
                }

                return tagName;
            }
        }

        public string ClassName => ClassCode != null && ModuleFactory.HaloInfiniteClasses.TryGetValue(ClassCode, out var className) ? className : ClassCode;

        public string FileName => Utils.GetFileName(TagName);

        public ModuleItem(Module module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        #region IModuleItem

        IModule IModuleItem.Module => Module;
        IEnumerable<IModuleItem> IModuleItem.EnumerateResourceItems() => Enumerable.Range(ResourceIndex, ResourceCount).Select(i => Module.Items[Module.Resources[i]]);
        IMetadataHeader IModuleItem.ReadMetadataHeader(DependencyReader reader) => new MetadataHeader(reader);

        #endregion

        public DependencyReader CreateReader()
        {
            DependencyReader Register(DependencyReader r)
            {
                r.RegisterInstance(this);
                r.RegisterInstance<IModuleItem>(this);
                return r;
            }

            using (var reader = Module.CreateReader(DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1)))
            {
                // HD1 Delta is the offset of which the same data is found in the hd1 handle.
                var fileOffset = DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1)
                    ? DataOffset - Module.Header.HD1Delta
                    : DataOffset + Module.DataAddress;

                var fileBuffer = new MemoryStream(TotalUncompressedSize);

                if (BlockCount != 0)
                {
                    reader.Seek(fileOffset, SeekOrigin.Begin);

                    for (var i = BlockIndex; i < BlockIndex + BlockCount; i++)
                    {
                        var block = Module.Blocks[i];
                        if (block.Compressed == 1)
                        {
                            var blockBuffer = new byte[block.CompressedSize];
                            reader.Read(blockBuffer, 0, block.CompressedSize);

                            var decompressedData = Oodle.Decompress(blockBuffer, blockBuffer.Length, block.UncompressedSize);
                            fileBuffer.Write(decompressedData, 0, decompressedData.Length);
                        }
                        else
                        {
                            var blockBuffer = new byte[block.UncompressedSize];
                            reader.Read(blockBuffer, 0, block.UncompressedSize);
                            fileBuffer.Write(blockBuffer);
                        }
                    }
                }
                else
                {
                    var blockBuffer = new byte[TotalCompressedSize];

                    reader.Seek(fileOffset, SeekOrigin.Begin);
                    reader.Read(blockBuffer, 0, TotalCompressedSize);

                    if (TotalCompressedSize == TotalUncompressedSize)
                        fileBuffer.Write(blockBuffer);
                    else
                    {
                        var decompressedData = Oodle.Decompress(blockBuffer, TotalCompressedSize, TotalUncompressedSize);
                        fileBuffer.Write(decompressedData);
                    }
                }

                fileBuffer.Position = 0;
                return Register(Module.CreateReader(fileBuffer));
            }
        }

        public T ReadMetadata<T>()
        {
            using (var reader = CreateReader())
            {
                var header = new MetadataHeader(reader); //MetadataHeader self registers to reader

                using (var vreader = (DependencyReader)reader.CreateVirtualReader(header.Header.HeaderSize))
                {
                    var mainBlock = header.StructureDefinitions.First(s => s.Type == StructureType.Main).TargetIndex;

                    vreader.Seek(header.DataBlocks[mainBlock].Offset, SeekOrigin.Begin);
                    var result = vreader.ReadObject<T>();

                    BlockHelper.LoadBlockCollections(result, mainBlock, vreader);

                    return result;
                }
            }
        }
    }
}
