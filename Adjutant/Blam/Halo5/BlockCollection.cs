using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo5
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

            reader.Seek(block.Offset, SeekOrigin.Begin);
            for (int i = 0; i < blockCount; i++)
                Add(reader.ReadObject<T>());

            var blockProps = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(BlockCollection<>));

            foreach (var item in this)
            {
                foreach (var prop in blockProps)
                {
                    var collection = prop.GetValue(item) as IBlockCollection;
                    var offset = OffsetAttribute.ValueFor(prop);
                    collection.LoadBlocks(structdef.TargetIndex, offset, reader);
                }
            }
        }
    }
}
