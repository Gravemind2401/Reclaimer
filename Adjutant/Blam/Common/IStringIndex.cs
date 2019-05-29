using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    public interface IStringIndex : IEnumerable<string>
    {
        int StringCount { get; }
        string this[int id] { get; }
    }
}
