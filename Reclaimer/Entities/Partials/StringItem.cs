using Adjutant.Blam.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Entities
{
    public partial class StringItem : IStringItem
    {
        int IStringItem.Id => (int)StringId;

        string IStringItem.Value => StringValue?.Value;
    }
}
