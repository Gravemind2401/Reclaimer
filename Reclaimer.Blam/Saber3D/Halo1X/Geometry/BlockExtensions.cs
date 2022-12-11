using Reclaimer.IO;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public static class BlockExtensions
    {
        internal static readonly Dictionary<Type, DataBlockAttribute> AttributeLookup = typeof(DataBlock).Assembly.GetTypes()
            .Where(t => typeof(DataBlock).IsAssignableFrom(t) && Attribute.IsDefined(t, typeof(DataBlockAttribute)))
            .ToDictionary(t => t, t => t.GetCustomAttribute<DataBlockAttribute>());

        private static readonly Dictionary<int, Type> blockLookup = AttributeLookup.ToDictionary(kv => kv.Value.BlockType, kv => kv.Key);

        public static DataBlock ReadBlock(this EndianReader reader, INodeGraph owner) => ReadBlock(reader, owner, null);
        public static DataBlock ReadBlock(this EndianReader reader, INodeGraph owner, DataBlock parent)
        {
            var typeId = reader.ReadUInt16(ByteOrder.BigEndian); //always read LTR
            var block = (DataBlock)Activator.CreateInstance(blockLookup.GetValueOrDefault(typeId) ?? typeof(DataBlock));
            block.SetOwner(owner);
            block.SetParent(parent);

            block.Header.StartOfBlock = (int)reader.Position - 2; //adjust for typeId already being read
            block.Header.BlockType = typeId;
            block.Header.EndOfBlock = reader.ReadInt32();

            block.Read(reader);
            block.Validate();

            if (block.ExpectedSize >= 0 && block.Header.BlockSize != block.ExpectedSize)
                Debugger.Break();

            reader.Seek(block.Header.EndOfBlock, SeekOrigin.Begin);

            return block;
        }

        public static Matrix4x4 ReadMatrix4x4(this EndianReader reader)
        {
            return new Matrix4x4
            {
                M11 = reader.ReadSingle(),
                M12 = reader.ReadSingle(),
                M13 = reader.ReadSingle(),
                M14 = reader.ReadSingle(),
                M21 = reader.ReadSingle(),
                M22 = reader.ReadSingle(),
                M23 = reader.ReadSingle(),
                M24 = reader.ReadSingle(),
                M31 = reader.ReadSingle(),
                M32 = reader.ReadSingle(),
                M33 = reader.ReadSingle(),
                M34 = reader.ReadSingle(),
                M41 = reader.ReadSingle(),
                M42 = reader.ReadSingle(),
                M43 = reader.ReadSingle(),
                M44 = reader.ReadSingle()
            };
        }
    }
}
