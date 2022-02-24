using Reclaimer.Blam.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface IMccGen3Header : IGen3Header
    {
        int StringNamespaceCount { get; set; }
        Pointer StringNamespaceTablePointer { get; set; }
    }
}
