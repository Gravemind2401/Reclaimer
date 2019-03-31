using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Reclaimer.Entities
{
    public partial class TagIndex : ITagIndex<TagItem>
    {
        TagItem ITagIndex<TagItem>.this[int index]
        {
            get
            {
                return TagItems.First(i => i.TagId == index);
            }
        }

        IEnumerator<TagItem> IEnumerable<TagItem>.GetEnumerator()
        {
            return TagItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return TagItems.GetEnumerator();
        }
    }
}
