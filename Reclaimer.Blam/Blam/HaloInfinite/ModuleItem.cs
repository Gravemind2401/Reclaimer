using OodleSharp;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;
using System.Text;

namespace Reclaimer.Blam.HaloInfinite
{
    public enum DataOffsetFlags  : byte
    {
        UseHD1 = 0b00000001,
        UseHD2 = 0b00000010,
    }

    [FixedSize(88)]
    public class ModuleItem
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

        public string ClassCode => (ClassId == -1) ? null : Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId));

        public long DataOffset => DataOffsetTemp & 0x0000FFFFFFFFFFFF;
        public DataOffsetFlags DataOffsetFlags => (DataOffsetFlags)(DataOffsetTemp >> 48);
        public string TagName => TagMapper.TagMappings.TryGetValue(GlobalTagId, out var value) && 
            !string.IsNullOrEmpty(value)
            ? value : GlobalTagId.ToString();
        public string ClassName => ClassCode;

        public string FileName => Utils.GetFileName(TagName);

        public ModuleItem(Module module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        public DependencyReader CreateReader()
        {
            DependencyReader Register(DependencyReader r)
            {
                r.RegisterInstance(this);
                return r;
            }

            using (var reader = Module.CreateReader())
            {
                var file_offset = Module.DataAddress + DataOffset;
                var file_buffer = new MemoryStream(TotalUncompressedSize);

                if (BlockCount != 0)
                {
                    reader.Seek(file_offset, SeekOrigin.Begin);

                    for (int i = BlockIndex; i < BlockIndex + BlockCount; i++)
                    {
                        var block = Module.Blocks[i];
                        if (block.Compressed == 1)
                        {
                            var block_buffer = new byte[block.CompressedSize];
                            reader.Read(block_buffer, 0, block.CompressedSize);

                            byte[] decompressed_data = Oodle.Decompress(block_buffer, block_buffer.Length, block.UncompressedSize);
                            file_buffer.Write(decompressed_data, 0, decompressed_data.Length);
                        } else
                        {
                            var block_buffer = new byte[block.UncompressedSize];
                            reader.Read(block_buffer, 0, block.UncompressedSize);
                            file_buffer.Write(block_buffer);
                        }
                    }
                } else
                {
                    reader.Seek(file_offset, SeekOrigin.Begin);
                    var block_buffer = new byte[TotalCompressedSize];
                    reader.Read(block_buffer, 0, TotalCompressedSize);

                    if (TotalCompressedSize == TotalUncompressedSize)
                    {
                        file_buffer.Write(block_buffer);
                    } else
                    {
                        byte[] decompressed_data = Oodle.Decompress(block_buffer, TotalCompressedSize, TotalUncompressedSize);
                        file_buffer.Write(decompressed_data);
                    }
                }

                file_buffer.Position = 0;
                return Register(Module.CreateReader(file_buffer));
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

                    var blockProps = typeof(T).GetProperties()
                        .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(BlockCollection<>));

                    foreach (var prop in blockProps)
                    {
                        var collection = prop.GetValue(result) as IBlockCollection;
                        var offset = OffsetAttribute.ValueFor(prop);
                        collection.LoadBlocks(mainBlock, offset, vreader);
                    }

                    return result;
                }
            }
        }
    }
}
