using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Adjutant.Blam.Common.Gen3
{
    public class LocaleIndex
    {
        private readonly Dictionary<int, LocaleTable> languages;

        public LocaleIndex(IGen3CacheFile cache, int offset, int size, int count)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            languages = new Dictionary<int, LocaleTable>();

            var globalsTag = cache.TagIndex.GetGlobalTag("matg");
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                for (int i = 0; i < count; i++)
                {
                    reader.Seek(globalsTag.MetaPointer.Address + offset + i * size, SeekOrigin.Begin);
                    var definition = reader.ReadObject<LanguageDefinition>();
                    languages.Add(i, new LocaleTable(cache, definition));
                }
            }
        }

        public LocaleTable this[Language lang] => languages.ValueOrDefault((int)lang);
    }

    public class LocaleTable : IReadOnlyList<string>
    {
        private readonly IGen3CacheFile cache;
        private readonly LanguageDefinition definition;
        private readonly string[] values;

        private bool isInitialised = false;

        public int Count => values.Length;

        public string this[int index]
        {
            get
            {
                if (!isInitialised)
                    ReadItems();

                return values[index];
            }
        }

        public LocaleTable(IGen3CacheFile cache, LanguageDefinition definition)
        {
            this.cache = cache;
            this.definition = definition;
            values = new string[definition.StringCount];
        }

        private void ReadItems()
        {
            var translator = new SectionAddressTranslator(cache, 3);
            using (var reader = cache.CreateReader(translator))
            {
                var addr = translator.GetAddress(definition.IndicesOffset);
                reader.Seek(addr, SeekOrigin.Begin);
                var indices = reader.ReadEnumerable<LocaleStringIndex>(definition.StringCount).ToList();

                addr = translator.GetAddress(definition.StringsOffset);
                using (var tempReader = reader.CreateVirtualReader(addr))
                {
                    for (int i = 0; i < definition.StringCount; i++)
                    {
                        if (indices[i].Offset < 0)
                            continue;

                        tempReader.Seek(indices[i].Offset, SeekOrigin.Begin);
                        values[i] = tempReader.ReadNullTerminatedString();
                    }
                }
            }

            isInitialised = true;
        }

        #region IEnumerable
        public IEnumerator<string> GetEnumerator()
        {
            if (!isInitialised)
                ReadItems();

            return ((IReadOnlyList<string>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!isInitialised)
                ReadItems();

            return ((IReadOnlyList<string>)values).GetEnumerator();
        } 
        #endregion

        [FixedSize(8)]
        public class LocaleStringIndex
        {
            [Offset(0)]
            public StringId StringId { get; set; }

            [Offset(4)]
            public int Offset { get; set; }
        }
    }
}