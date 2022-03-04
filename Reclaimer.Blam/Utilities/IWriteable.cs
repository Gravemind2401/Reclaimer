using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    public interface IWriteable
    {
        void Write(EndianWriter writer, double? version);
    }
}
