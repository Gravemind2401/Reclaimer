using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public static class BlockHelper
    {
        public static void LoadBlockCollections<T>(T instance, int sourceBlockIndex, DependencyReader tagBodyReader)
        {
            ArgumentNullException.ThrowIfNull(instance);

            var blockProps = from p in typeof(T).GetProperties()
                             where p.PropertyType.IsGenericType
                             && p.PropertyType.GetGenericTypeDefinition() == typeof(BlockCollection<>)
                             select p;

            foreach (var prop in blockProps)
            {
                var collection = prop.GetValue(instance) as IBlockCollection;
                var offset = OffsetAttribute.ValueFor(prop);
                collection.LoadBlocks(sourceBlockIndex, offset, tagBodyReader);
            }
        }
    }
}
