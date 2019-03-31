using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Reclaimer.Entities
{
    public partial class TagIndex : ITagIndex<IndexItem>
    {
        IndexItem ITagIndex<IndexItem>.this[int index]
        {
            get
            {
                return IndexItems.First(i => i.TagId == index);
            }
        }

        IEnumerator<IndexItem> IEnumerable<IndexItem>.GetEnumerator()
        {
            return IndexItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return IndexItems.GetEnumerator();
        }
    }
}
