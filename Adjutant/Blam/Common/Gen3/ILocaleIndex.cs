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
    }

    public interface ILocaleTable
    {
        string this[int index] { get; }

        int StringCount { get; set; }
        int StringsSize { get; set; }
        int IndicesOffset { get; set; }
        int StringsOffset { get; set; }
    }
}
