using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Reclaimer.Entities
{
    public partial class StringIndex : IStringIndex<StringItem>
    {
        StringItem IStringIndex<StringItem>.this[int id]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IEnumerator<StringItem> IEnumerable<StringItem>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
