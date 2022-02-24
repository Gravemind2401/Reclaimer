using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Audio
{
    public interface IFormatHeader
    {
        int Length { get; }
        byte[] GetBytes();
    }
}
