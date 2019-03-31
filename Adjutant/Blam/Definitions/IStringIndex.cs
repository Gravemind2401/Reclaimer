using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Definitions
{
    public interface IStringIndex<out TStringItem> : IEnumerable<TStringItem> where TStringItem : IStringItem
    {
        int StringCount { get; }
        TStringItem this[int id] { get; }
    }
}
