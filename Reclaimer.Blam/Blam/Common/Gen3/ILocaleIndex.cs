using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface ILocaleIndex : IWriteable
    {
        ILocaleTable this[Language lang] { get; }
        string this[Language lang, StringId key] { get; }
        IReadOnlyList<ILocaleTable> Languages { get; }
    }

    public interface ILocaleTable : IEnumerable<KeyValuePair<StringId, string>>
    {
        Language Language { get; }

        string this[StringId key] { get; }

        int StringCount { get; set; }
        int StringsSize { get; set; }
        int IndicesOffset { get; set; }
        int StringsOffset { get; set; }
    }
}