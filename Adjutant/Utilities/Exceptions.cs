using System;
using System.Collections.Generic;
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
    }
}
