using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Definitions
{
    public interface ICacheFile
    {
        string FileName { get; }
        string BuildString { get; }
        CacheType Type { get; }

        string GetString(int id);
    }
}
