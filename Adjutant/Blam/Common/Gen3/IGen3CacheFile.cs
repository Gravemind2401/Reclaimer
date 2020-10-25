using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public interface IGen3CacheFile : ICacheFile
    {
        long VirtualBaseAddress { get; }
        SectionOffsetTable SectionOffsetTable { get; }
        SectionTable SectionTable { get; }
    }
}
