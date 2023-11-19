using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Reclaimer.Geometry.Utilities
{
    /// <typeparam name="TKey">
    /// The type of the key that elements will be compared with.
    /// </typeparam>
    /// <inheritdoc cref="LazyList{TValue}"/>
    internal class LazyList<TKey, TValue> : LazyList<TValue>
        where TKey : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyList{TKey, TValue}"/> class that compares elements using the results provided by the <paramref name="keySelector"/> function.
        /// </summary>
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

    /// <summary>
    /// A list that can only contain unique values.
    /// <br/> When <see cref="IndexOf(TValue)"/> is called, the value will automatically be added to the list, if not already present.
    /// <br/> The list is readonly, except for adding new values or clearing the list entirely.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of elements in the list.
    /// </typeparam>
    internal class LazyList<TValue> : IList<TValue>
    {
        private readonly Dictionary<TValue, int> indexLookup;
        private readonly List<TValue> valueList = new();

        public int Count => valueList.Count;
        public bool IsReadOnly => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyList{TValue}"/> class that uses the default equality comparer for the element type.
        /// </summary>
        public LazyList()
            : this(EqualityComparer<TValue>.Default)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyList{TValue}"/> class that uses the specified <see cref="IEqualityComparer{T}"/>.
        /// </summary>
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

        /// <remarks>
        /// This has no effect if the specified value is already present in the list.
        /// </remarks>
        /// <inheritdoc cref="ICollection{T}.Add(T)"/>
        public void Add(TValue item) => AddIfNew(item);

        public bool Contains(TValue item) => indexLookup.ContainsKey(item);

        /// <remarks>
        /// The specified value will be added to the list if not already present.
        /// </remarks>
        /// <inheritdoc cref="IList{T}.IndexOf(T)"/>
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
