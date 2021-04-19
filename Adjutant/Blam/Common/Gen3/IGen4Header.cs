using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public interface IGen4Header : IGen3Header
    {
        int UnknownTableSize { get; set; }
        Pointer UnknownTablePointer { get; set; }
    }
}
