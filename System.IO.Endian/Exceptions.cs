using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    internal static class Error
    {
        #region Generic Errors

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

        #endregion

        #region Specific Errors

        /// <exception cref="MissingMethodException"/>
        internal static void MissingPrimitiveReadMethod(string typeName)
        {
            throw new MissingMethodException($"{nameof(EndianReader)} does not have a primitive read function for {typeName} values.");
        }

        /// <exception cref="MissingMethodException"/>
        internal static void MissingPrimitiveWriteMethod(string typeName)
        {
            throw new MissingMethodException($"{nameof(EndianWriter)} does not have a primitive write function for {typeName} values.");
        }

        /// <exception cref="AmbiguousMatchException"/>
        internal static void AttributeVersionOverlap(string memberName, string attrName, double? version)
        {
            throw new AmbiguousMatchException($"The type or property '{memberName}' has multiple {attrName}s specified that are a match for version '{version?.ToString() ?? "null"}'.");
        }

        /// <exception cref="AmbiguousMatchException"/>
        internal static void StringTypeOverlap(string propName)
        {
            throw new AmbiguousMatchException($"The {propName} property has multiple string type specifier attributes applied.");
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        internal static void NoVersionSpecified()
        {
            throw new ArgumentException($"At least one version boundary must be specified.");
        }

        /// <exception cref="ArgumentException"/>
        internal static void NotValidForPrimitiveTypes([CallerMemberName]string methodName = null)
        {
            throw new ArgumentException($"{methodName} should not be used on primitive types or strings.");
        }

        #endregion
    }
}
