using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Reclaimer.Blam.HaloInfinite
{
    internal interface IBlockCollection
    {
        void LoadBlocks(int currentBlock, long collectionOffset, DependencyReader itemReader);
    }

    [FixedSize(20)]
    public class BlockCollection<T> : Collection<T>, IBlockCollection
    {
        private readonly uint blockCount;
        private readonly MetadataHeader metadata;

        public BlockCollection(DependencyReader reader, MetadataHeader metadata)
        {
            this.metadata = metadata;

            reader.Seek(16, SeekOrigin.Current);
            blockCount = reader.ReadUInt32();
        }

        void IBlockCollection.LoadBlocks(int currentBlock, long collectionOffset, DependencyReader itemReader) => LoadBlocks(currentBlock, collectionOffset, itemReader);

        internal void LoadBlocks(int currentBlock, long collectionOffset, DependencyReader reader)
        {
            if (blockCount == 0)
                return;

            var containingStructDef = metadata.StructureDefinitions.FirstOrDefault(s => s.FieldBlock == currentBlock && s.FieldOffset == collectionOffset && s.TargetIndex != -1);
            if (containingStructDef == null || containingStructDef.TargetIndex == -1)
                return;

            var block = metadata.DataBlocks[containingStructDef.TargetIndex];
            var blockSize = typeof(T).IsPrimitive ? Marshal.SizeOf<T>() : (int)FixedSizeAttribute.ValueFor(typeof(T), (double)metadata.Header.Version);

            reader.Seek(block.Offset, SeekOrigin.Begin);
            for (var i = 0; i < blockCount; i++)
                Add(reader.ReadObject<T>());

            var blockProps = from p in typeof(T).GetProperties()
                             where p.PropertyType.IsGenericType
                             && p.PropertyType.GetGenericTypeDefinition() == typeof(BlockCollection<>)
                             select p;

            var structProps = from p in typeof(T).GetProperties()
                              let structDefAttr = p.GetCustomAttribute<LoadFromStructureDefinitionAttribute>()
                              where structDefAttr != null
                              select (p, structDefAttr.StructureGuid);

            var index = 0;
            foreach (var item in this)
            {
                var adjustedBase = blockSize * index++;
                foreach (var prop in blockProps)
                {
                    var collection = prop.GetValue(item) as IBlockCollection;
                    var offset = OffsetAttribute.ValueFor(prop);
                    collection.LoadBlocks(containingStructDef.TargetIndex, adjustedBase + offset, reader);
                }

                foreach (var (prop, structureGuid) in structProps)
                {
                    var propStructDef = metadata.StructureDefinitions.SingleOrDefault(s => s.FieldBlock == containingStructDef.TargetIndex && s.Guid == structureGuid);
                    if (propStructDef == null)
                        continue;

                    object propValue;

                    var structBlock = metadata.DataBlocks[propStructDef.TargetIndex];
                    using (var dataReader = reader.CreateVirtualReader(metadata.GetSectionOffset(structBlock.Section)))
                    {
                        dataReader.Seek(structBlock.Offset, SeekOrigin.Begin);
                        propValue = dataReader.ReadObject(prop.PropertyType);
                    }

                    //need to invoke this as a generic for the specific target property type
                    //calling the method directly wont use the correct type since "propValue" is currently just typed as object
                    var loadBlocksGeneric = typeof(BlockHelper)
                        .GetMethod(nameof(BlockHelper.LoadBlockCollections), BindingFlags.Public | BindingFlags.Static)
                        .GetGenericMethodDefinition()
                        .MakeGenericMethod(prop.PropertyType);

                    loadBlocksGeneric.Invoke(null, new object[] { propValue, propStructDef.TargetIndex, reader });
                    prop.SetValue(item, propValue);
                }
            }
        }
    }
}
