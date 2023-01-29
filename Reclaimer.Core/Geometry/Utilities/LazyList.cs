using System.Collections;

namespace Reclaimer.Geometry.Utilities
{
    internal class LazyList<TValue> : LazyList<TValue, TValue>
    {
        public LazyList()
            : base(obj => obj)
        { }
    }

    internal class LazyList<TKey, TValue> : IList<TValue>
    {
        private readonly Func<TValue, TKey> keySelector;
        private readonly List<TKey> keyList = new();
        private readonly Dictionary<TKey, TValue> valueLookup = new();

        public int Count => keyList.Count;
        public bool IsReadOnly => true;

        public LazyList(Func<TValue, TKey> keySelector)
        {
            this.keySelector = keySelector;
        }

        public TValue this[int index]
        {
            get => valueLookup[keyList[index]];
            set => throw new NotSupportedException();
        }

        private TKey AddIfNew(TValue value)
        {
            var key = keySelector(value);
            if (!valueLookup.ContainsKey(key))
            {
                valueLookup.Add(key, value);
                keyList.Add(key);
            }

            return key;
        }

        public void AddRange(IEnumerable<TValue> items)
        {
            foreach (var item in items)
                AddIfNew(item);
        }

        public void Add(TValue item) => AddIfNew(item);
        public bool Contains(TValue item) => valueLookup.ContainsKey(keySelector(item));

        public int IndexOf(TValue value)
        {
            var key = AddIfNew(value);
            return keyList.IndexOf(key);
        }

        public void Clear()
        {
            keyList.Clear();
            valueLookup.Clear();
        }

        public IEnumerator<TValue> GetEnumerator() => keyList.Select(k => valueLookup[k]).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this).GetEnumerator();

        void IList<TValue>.Insert(int index, TValue item) => throw new NotSupportedException();
        void IList<TValue>.RemoveAt(int index) => throw new NotSupportedException();
        void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex) => throw new NotSupportedException();
        bool ICollection<TValue>.Remove(TValue item) => throw new NotSupportedException();
    }
}
