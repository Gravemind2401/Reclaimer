using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Reclaimer.Blam.Halo5
{
    internal interface IBlockCollection
    {
        void LoadBlocks(int currentBlock, long collectionOffset, DependencyReader itemReader);
    }

    [FixedSize(28)]
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

            var structdef = metadata.StructureDefinitions.First(s => s.FieldBlock == currentBlock && s.FieldOffset == collectionOffset);
            if (structdef.TargetIndex < 0)
                return;

            var block = metadata.DataBlocks[structdef.TargetIndex];

            var blockSize = FixedSizeAttribute.ValueFor(typeof(T));

            reader.Seek(block.Offset, SeekOrigin.Begin);
            for (var i = 0; i < blockCount; i++)
                Add(reader.ReadObject<T>());

            var blockProps = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(BlockCollection<>));

            var index = 0;
            foreach (var item in this)
            {
                var adjustedBase = blockSize * index++;
                foreach (var prop in blockProps)
                {
                    var collection = prop.GetValue(item) as IBlockCollection;
                    var offset = OffsetAttribute.ValueFor(prop);
                    collection.LoadBlocks(structdef.TargetIndex, adjustedBase + offset, reader);
                }
            }
        }
    }
}
