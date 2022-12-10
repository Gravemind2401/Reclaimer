using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reclaimer.Blam.Common.Gen3
{
    public class LocaleIndex : ILocaleIndex
    {
        private readonly int offset;
        private readonly int size;

        private readonly List<LanguageDefinition> definitions;
        private readonly List<LocaleTable> tables;

        public LocaleIndex(IGen3CacheFile cache, int offset, int size, int count)
        {
            if (cache == null)
                throw new ArgumentNullException(nameof(cache));

            this.offset = offset;
            this.size = size;

            definitions = new List<LanguageDefinition>();
            tables = new List<LocaleTable>();

            var globalsTag = cache.TagIndex.GetGlobalTag("matg");
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                for (var i = 0; i < count; i++)
                {
                    reader.Seek(globalsTag.MetaPointer.Address + offset + i * size, SeekOrigin.Begin);
                    var definition = reader.ReadObject<LanguageDefinition>();
                    definitions.Add(definition);
                    tables.Add(new LocaleTable(cache, definition, (Language)i));
                }
            }
        }

        public IReadOnlyList<ILocaleTable> Languages => tables;

        public LocaleTable this[Language lang] => tables.ElementAtOrDefault((int)lang);

        public string this[Language lang, StringId key] => tables.ElementAtOrDefault((int)lang)?[key];

        ILocaleTable ILocaleIndex.this[Language lang] => this[lang];

        void IWriteable.Write(EndianWriter writer, double? version)
        {
            var origin = writer.BaseStream.Position;

            for (var i = 0; i < definitions.Count; i++)
            {
                writer.Seek(origin + offset + i * size, SeekOrigin.Begin);
                writer.WriteObject(definitions[i]);
            }
        }
    }

    public class LocaleTable : ILocaleTable, IEnumerable<KeyValuePair<StringId, string>>
    {
        private readonly IGen3CacheFile cache;
        private readonly LanguageDefinition definition;
        private readonly Dictionary<int, List<string>> values;

        private bool isInitialised = false;

        public Language Language { get; }

        public int Count => definition.StringCount;

        public string this[StringId key]
        {
            get
            {
                if (!isInitialised)
                    ReadItems();

                return values.ContainsKey(key.Id) ? values[key.Id][0] : null;
            }
        }

        public LocaleTable(IGen3CacheFile cache, LanguageDefinition definition, Language lang)
        {
            this.cache = cache;
            this.definition = definition;
            values = new Dictionary<int, List<string>>(definition.StringCount);

            Language = lang;
        }

        private void ReadItems()
        {
            string key;
            switch (cache.CacheType)
            {
                case CacheType.HaloReachBeta:
                    key = HaloReach.CacheFile.BetaKey;
                    break;
                case CacheType.HaloReachRetail:
                    key = HaloReach.CacheFile.LocalesKey;
                    break;
                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
                    key = Halo4.CacheFile.LocalesKey;
                    break;

                default:
                    key = null;
                    break;
            }

            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                var translator = new SectionAddressTranslator(cache, 3);

                reader.Seek((int)translator.GetAddress(definition.IndicesOffset), SeekOrigin.Begin);
                var entries = reader.ReadArray<LocaleEntry>(definition.StringCount);

                reader.Seek((int)translator.GetAddress(definition.StringsOffset), SeekOrigin.Begin);

                Stream ms = null;
                EndianReader tempReader;

                if (!string.IsNullOrEmpty(key))
                {
                    var decrypted = reader.ReadAesBytes(definition.StringsSize, key);
                    ms = new MemoryStream(decrypted);
                    tempReader = new EndianReader(ms);
                }
                else
                    tempReader = reader.CreateVirtualReader();

                for (var i = 0; i < definition.StringCount; i++)
                {
                    if (entries[i].Offset < 0)
                        continue;

                    //why are there duplicate stringids?
                    var id = entries[i].StringId.Id;
                    if (!values.ContainsKey(id))
                        values.Add(id, new List<string>());

                    tempReader.Seek(entries[i].Offset, SeekOrigin.Begin);
                    values[id].Add(tempReader.ReadNullTerminatedString());
                }

                ms?.Dispose();
                tempReader.Dispose();
            }

            isInitialised = true;
        }

        #region IEnumerable
        public IEnumerator<KeyValuePair<StringId, string>> GetEnumerator()
        {
            if (!isInitialised)
                ReadItems();

            return ((IReadOnlyList<KeyValuePair<StringId, string>>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (!isInitialised)
                ReadItems();

            return ((IReadOnlyList<KeyValuePair<StringId, string>>)values).GetEnumerator();
        }
        #endregion

        #region ILocaleTable
        string ILocaleTable.this[StringId key] => this[key];

        int ILocaleTable.StringCount
        {
            get => definition.StringCount;
            set => definition.StringCount = value;
        }

        int ILocaleTable.StringsSize
        {
            get => definition.StringsSize;
            set => definition.StringsSize = value;
        }

        int ILocaleTable.IndicesOffset
        {
            get => definition.IndicesOffset;
            set => definition.IndicesOffset = value;
        }

        int ILocaleTable.StringsOffset
        {
            get => definition.StringsOffset;
            set => definition.StringsOffset = value;
        }
        #endregion

        [FixedSize(8)]
        public class LocaleEntry //must be public for dynamic reader to instanciate
        {
            [Offset(0)]
            public StringId StringId { get; set; }

            [Offset(4)]
            public int Offset { get; set; }

            public override string ToString() => $"{StringId.Id}: {Offset}";
        }
    }
}