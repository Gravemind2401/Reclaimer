using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utils
{
    public class StandardAddressTranslator : IAddressTranslator
    {
        public int Magic { get; }

        public StandardAddressTranslator(int magic)
        {
            Magic = magic;
        }

        public int GetAddress(int pointer)
        {
            return pointer - Magic;
        }

        public int GetPointer(int address)
        {
            return address + Magic;
        }
    }
}
