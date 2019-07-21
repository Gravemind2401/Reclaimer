using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    internal static class Exceptions
    {
        public static NotSupportedException TagClassNotSupported(IIndexItem item)
        {
            return new NotSupportedException($"{item.CacheFile.CacheType} {item.ClassName} tags are not supported");
        }
    }
}
