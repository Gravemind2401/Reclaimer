using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IAddressTranslator
    {
        long GetAddress(long pointer);
        long GetPointer(long address);
    }
}
