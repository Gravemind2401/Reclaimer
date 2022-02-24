using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface IGen4Header : IGen3Header
    {
        int UnknownTableSize { get; set; }
        Pointer UnknownTablePointer { get; set; }
    }
}
