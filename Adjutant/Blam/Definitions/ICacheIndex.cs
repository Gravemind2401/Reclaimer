using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Definitions
{
    public interface ICacheIndex<out TIndexItem> : IEnumerable<TIndexItem> where TIndexItem : IIndexItem
    {
        int TagCount { get; }
        TIndexItem this[int index] { get; }
    }
}
