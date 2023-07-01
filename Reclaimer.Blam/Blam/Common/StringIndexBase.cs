using System.Collections;

namespace Reclaimer.Blam.Common
{
    public abstract class StringIndexBase : IEnumerable<string>, IStringIndex
    {
        protected string[] Items { get; init; }

        protected virtual int GetStringIndex(int id) => id;

        public int StringCount => Items.Length;

        public string this[int id] => TryGetValue(id, out var value) ? value : null;

        public bool TryGetValue(int id, out string value)
        {
            id = GetStringIndex(id);
            if (id < 0 || id >= Items.Length)
            {
                value = default;
                return false;
            }

            value = Items[id];
            return true;
        }

        public abstract int GetStringId(string value);

        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)Items).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
    }
}
