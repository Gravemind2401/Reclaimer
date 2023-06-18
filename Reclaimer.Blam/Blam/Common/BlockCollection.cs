using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System.Collections;
using System.Collections.ObjectModel;

namespace Reclaimer.Blam.Common
{
    [FixedSize(12, MaxVersion = (int)CacheType.Halo2Xbox)]
    [FixedSize(8, MinVersion = (int)CacheType.Halo2Xbox, MaxVersion = (int)CacheType.Halo3Beta)]
    [FixedSize(12, MinVersion = (int)CacheType.Halo3Beta)]
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class BlockCollection<T> : TagBlock, IReadOnlyList<T>
    {
        private readonly Collection<T> items = new Collection<T>();

        public BlockCollection(DependencyReader reader, ICacheFile cache, IAddressTranslator translator)
            : this(reader, cache, translator, null)
        { }

        public BlockCollection(DependencyReader reader, ICacheFile cache, IAddressTranslator translator, IPointerExpander expander)
            : base(reader, cache, translator, expander)
        {
            if (IsInvalid)
                return;

            reader.BaseStream.Position = Pointer.Address;
            for (var i = 0; i < Count; i++)
                items.Add((T)reader.ReadObject(typeof(T), (int)cache.CacheType));
        }

        private string GetDebuggerDisplay() => $"{nameof(items.Count)} = {items.Count}";

        #region IReadOnlyList
        public T this[int index] => items[index];

        public int IndexOf(T item) => items.IndexOf(item);

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
        #endregion
    }
}