using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public interface ILocaleIndex
    {
        ILocaleTable this[Language lang] { get; }
        string this[Language lang, StringId key] { get; }
    }

    public interface ILocaleTable : IEnumerable<KeyValuePair<StringId, string>>
    {
        string this[StringId key] { get; }

        int StringCount { get; set; }
        int StringsSize { get; set; }
        int IndicesOffset { get; set; }
        int StringsOffset { get; set; }
    }
}
