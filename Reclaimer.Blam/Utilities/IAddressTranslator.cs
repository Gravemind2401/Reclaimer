using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public interface IAddressTranslator
    {
        long GetAddress(long pointer);
        long GetPointer(long address);
    }
}
