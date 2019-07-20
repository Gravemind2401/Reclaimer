using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    internal static class Exceptions
    {
        internal static ArgumentException ParamMustBeNonZero(string paramName)
        {
            return new ArgumentException(Utils.CurrentCulture($"{paramName} cannot be zero."), paramName);
        }

        internal static InvalidOperationException CoordSysNotConvertable()
        {
            return new InvalidOperationException(Utils.CurrentCulture($"No conversion exists between the given coordinate systems."));
        }

        internal static FileNotFoundException FileNotFound(string fileName)
        {
            return new FileNotFoundException(Utils.CurrentCulture($"The file does not exist."), fileName);
        }

        internal static ArgumentException NotAValidMapFile(string fileName)
        {
            return new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened as a map. It is invalid or unsupported."));
        }
    }
}
