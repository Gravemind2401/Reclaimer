using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    [CLSCompliant(false)]
    public interface IGen3CacheFile : ICacheFile
    {
        SectionOffsetTable SectionOffsetTable { get; }
        SectionTable SectionTable { get; }
    }
}
