using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Reclaimer.Geometry.Utilities
{
    internal class LazyList<TKey, TValue> : LazyList<TValue>
    {
        public LazyList(Func<TValue, TKey> keySelector)
            : base(new LambdaEqualityComparer(keySelector))
        { }

        private sealed class LambdaEqualityComparer : IEqualityComparer<TValue>
        {
            private readonly Func<TValue, TKey> keySelector;

            public LambdaEqualityComparer(Func<TValue, TKey> keySelector)
            {
                this.keySelector = keySelector;
            }

            public bool Equals(TValue x, TValue y) => keySelector(x).Equals(keySelector(y));
            public int GetHashCode([DisallowNull] TValue obj) => keySelector(obj).GetHashCode();
        }
    }

    internal class LazyList<TValue> : IList<TValue>
    {
        private readonly Dictionary<TValue, int> indexLookup;
        private readonly List<TValue> valueList = new();

        public int Count => valueList.Count;
        public bool IsReadOnly => true;

        public LazyList() : this(EqualityComparer<TValue>.Default)
        { }

        public LazyList(IEqualityComparer<TValue> comparer)
        {
            indexLookup = new Dictionary<TValue, int>(comparer);
        }

        public TValue this[int index]
        {
            get => valueList[index];
            set => throw new NotSupportedException();
        }

        private int AddIfNew(TValue value)
        {
            if (!indexLookup.TryGetValue(value, out var index))
            {
                indexLookup.Add(value, index = valueList.Count);
                valueList.Add(value);
            }

            return index;
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            foreach (var item in items)
                AddIfNew(item);
        }

        public void Add(TValue item) => AddIfNew(item);
        public bool Contains(TValue item) => indexLookup.ContainsKey(item);
        public int IndexOf(TValue value) => AddIfNew(value);

        public void Clear()
        {
            valueList.Clear();
            indexLookup.Clear();
        }

        public IEnumerator<TValue> GetEnumerator() => valueList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this).GetEnumerator();

        void IList<TValue>.Insert(int index, TValue item) => throw new NotSupportedException();
        void IList<TValue>.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => throw new NotSupportedException();
        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();
    }
}
