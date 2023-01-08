using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Reclaimer.Blam.Halo5
{
    [FixedSize(88)]
    public class ModuleItem
    {
        public Module Module { get; }

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
        public uint TotalCompressedSize { get; set; }

        [Offset(36)]
        public uint TotalUncompressedSize { get; set; }

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
        public uint UncompressedHeaderSize { get; set; }

        [Offset(72)]
        public uint UncompressedTagDataSize { get; set; }

        [Offset(76)]
        public uint UncompressedResourceDataSize { get; set; }

        [Offset(80)]
        public short HeaderBlockCount { get; set; }

        [Offset(82)]
        public short TagDataBlockCount { get; set; }

        [Offset(84)]
        public short ResourceBlockCount { get; set; }

        public string ClassCode => (ClassId == -1) ? null : Encoding.UTF8.GetString(BitConverter.GetBytes(ClassId));

        private string fileName => Module.Strings[NameOffset];

        public string FullPath
        {
            get
            {
                if (GlobalTagId == -1)
                    return fileName;

                var len = fileName.LastIndexOf('.');
                return fileName.Substring(0, len);
            }
        }

        public string ClassName
        {
            get
            {
                if (GlobalTagId == -1)
                    return null;

                var index = fileName.LastIndexOf('.') + 1;
                return fileName.Substring(index, fileName.Length - index);
            }
        }

        public ModuleItem(Module module)
        {
            Module = module ?? throw new ArgumentNullException(nameof(module));
        }

        private Block GetImpliedBlock()
        {
            return new Block
            {
                CompressedOffset = 0,
                CompressedSize = TotalCompressedSize,
                UncompressedOffset = 0,
                UncompressedSize = TotalUncompressedSize,
                Compressed = TotalUncompressedSize > TotalCompressedSize ? 1 : 0
            };
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
                var blocks = BlockCount > 0 ? Module.Blocks.Skip(BlockIndex).Take(BlockCount) : Enumerable.Repeat(GetImpliedBlock(), 1);
                var decompressed = new MemoryStream((int)blocks.Sum(b => b.UncompressedSize));

                foreach (var block in blocks)
                {
                    reader.Seek(Module.DataAddress + DataOffset + block.CompressedOffset, SeekOrigin.Begin);

                    if (block.Compressed == 0)
                        decompressed.Write(reader.ReadBytes((int)block.UncompressedSize), 0, (int)block.UncompressedSize);
                    else
                    {
                        using (var ms = new MemoryStream(reader.ReadBytes((int)block.CompressedSize)))
                        using (var zstream = new ZLibStream(ms, CompressionMode.Decompress))
                            zstream.CopyTo(decompressed);
                    }
                }

                decompressed.Position = 0;
                return Register(Module.CreateReader(decompressed));
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

        public override string ToString() => FullPath;
    }
}
