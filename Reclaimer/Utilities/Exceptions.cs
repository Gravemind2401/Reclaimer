using Reclaimer.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    internal static class Exceptions
    {
        public static NotSupportedException TagClassNotSupported(IIndexItem item) => new NotSupportedException($"{item.CacheFile.CacheType} {item.ClassName} tags are not supported");
        public static FileNotFoundException FileNotFound(string fileName) => new FileNotFoundException("The file does not exist.", fileName);
        public static ArgumentException MissingStringParameter(string paramName) => new ArgumentException("Parameter must not be null or whitespace", paramName);
    }
}
