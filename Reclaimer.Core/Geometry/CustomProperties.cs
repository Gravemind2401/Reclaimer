using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Reclaimer.Geometry
{
    public partial class CustomProperties : IReadOnlyDictionary<string, object>
    {
        private readonly Dictionary<string, object> properties = new();

        public IEnumerable<string> Keys => properties.Keys;
        public IEnumerable<object> Values => properties.Values;
        public int Count => properties.Count;

        public object this[string key] => properties[key];

        public bool ContainsKey(string key) => properties.ContainsKey(key);
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => properties.TryGetValue(key, out value);

        public void Add(string key, bool value) => AddObject(key, value);
        public void Add(string key, int value) => AddObject(key, value);
        public void Add(string key, float value) => AddObject(key, value);
        public void Add(string key, string value) => AddObject(key, value);

        public void Add(string key, IEnumerable<bool> value) => AddObject(key, value?.ToArray());
        public void Add(string key, IEnumerable<int> value) => AddObject(key, value?.ToArray());
        public void Add(string key, IEnumerable<float> value) => AddObject(key, value?.ToArray());
        public void Add(string key, IEnumerable<string> value) => AddObject(key, value?.ToArray());

        public void AddFromFlags<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>();
            foreach (var flag in values)
            {
                if (flag.Equals(default(TEnum)) || !value.HasFlag(flag))
                    continue;

                var key = Enum.GetName(flag);
                key = CaseChangeRegex().Replace(key, "_").ToLower();

                Add(key, true);
            }
        }

        private void AddObject(string key, object value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));
            if (value != null)
                properties[key] = value;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, object>>)properties).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)properties).GetEnumerator();

        [GeneratedRegex("(?<=[^A-Z])(?=[A-Z])")]
        private static partial Regex CaseChangeRegex();
    }
}
