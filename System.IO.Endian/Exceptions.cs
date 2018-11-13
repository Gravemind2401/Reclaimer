using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    internal static class Error
    {
        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void ParamMustBeNonNegative(string paramName, object paramValue)
        {
            throw new ArgumentOutOfRangeException(paramName, paramValue, $"The {paramName} value must be non-negative.");
        }
    }
}
