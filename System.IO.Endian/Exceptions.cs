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
        internal static void ParamMustBePositive(string paramName, object paramValue)
        {
            throw new ArgumentOutOfRangeException(paramName, paramValue, $"The {paramName} value must be greater than zero.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void ParamMustBeNonNegative(string paramName, object paramValue)
        {
            throw new ArgumentOutOfRangeException(paramName, paramValue, $"The {paramName} value must be non-negative.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void BoundaryOverlapAmbiguous(string minValue, string maxValue)
        {
            throw new ArgumentOutOfRangeException($"The {minValue} value cannot be greater than the {maxValue} value.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void BoundaryOverlapMinimum(string minValue, string maxValue)
        {
            throw new ArgumentOutOfRangeException($"{minValue} cannot be greater than {maxValue}.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void BoundaryOverlapMaximum(string minValue, string maxValue)
        {
            throw new ArgumentOutOfRangeException($"{maxValue} cannot be less than {minValue}.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void PropertyMustBeNullOrPositive(string paramName, object paramValue)
        {
            throw new ArgumentOutOfRangeException(paramName, paramValue, $"The {paramName} property must either be null or greater than zero.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void NoVersionSpecified()
        {
            throw new ArgumentException($"At least one version boundary must be specified.");
        }
    }
}
