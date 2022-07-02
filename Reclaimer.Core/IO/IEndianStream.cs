using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    internal interface IEndianStream
    {
        ByteOrder ByteOrder { get; }
        long Position { get; }
        void Seek(long offset, SeekOrigin origin);
    }
}
