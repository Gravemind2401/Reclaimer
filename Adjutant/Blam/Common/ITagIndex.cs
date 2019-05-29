using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    public interface ITagIndex<out TIndexItem> : IEnumerable<TIndexItem> where TIndexItem : IIndexItem
    {
        int TagCount { get; }
        TIndexItem this[int index] { get; }
    }
}
