using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Reclaimer.IO
{
    internal static class Exceptions
    {
        #region Generic Errors

        public static ArgumentOutOfRangeException ParamMustBePositive(object paramValue, [CallerArgumentExpression(nameof(paramValue))] string paramName = null)
        {
            return new ArgumentOutOfRangeException(paramName, paramValue, Utils.CurrentCulture($"The {paramName} value must be greater than zero."));
        }

        public static ArgumentOutOfRangeException ParamMustBeNonNegative(object paramValue, [CallerArgumentExpression(nameof(paramValue))] string paramName = null)
        {
            return new ArgumentOutOfRangeException(paramName, paramValue, Utils.CurrentCulture($"The {paramName} value must be non-negative."));
        }

        public static ArgumentOutOfRangeException BoundaryOverlapMinimum(string minValue, string maxValue) => new ArgumentOutOfRangeException(Utils.CurrentCulture($"{minValue} cannot be greater than {maxValue}."));
        public static ArgumentOutOfRangeException BoundaryOverlapMaximum(string minValue, string maxValue) => new ArgumentOutOfRangeException(Utils.CurrentCulture($"{maxValue} cannot be less than {minValue}."));
        public static ArgumentOutOfRangeException PropertyMustBeNullOrPositive(string paramName, object paramValue) => new ArgumentOutOfRangeException(paramName, paramValue, Utils.CurrentCulture($"The {paramName} property must either be null or greater than zero."));

        #endregion

        #region Specific Errors

        public static MissingMethodException TypeNotConstructable(string typeName) => new MissingMethodException(Utils.CurrentCulture($"Cannot create an object of type '{typeName}' because '{typeName}' does not have a public default constructor."));
        public static MissingMethodException NonPublicGetSet(string propName) => new MissingMethodException(Utils.CurrentCulture($"The '{propName}' property was marked for read/write but has no public get and/or set methods."));
        public static AmbiguousMatchException AttributeVersionOverlap(string memberName, Type attrType) => new AmbiguousMatchException(Utils.CurrentCulture($"The type or property '{memberName}' has multiple {attrType.Name}s specified that match the same read/write version."));
        public static AmbiguousMatchException StringTypeOverlap(string propName) => new AmbiguousMatchException(Utils.CurrentCulture($"The '{propName}' string property has multiple string type specifier attributes applied."));
        public static InvalidOperationException StringTypeUnknown(string propName) => new InvalidOperationException(Utils.CurrentCulture($"The '{propName}' string property was marked for read/write but does not have any string type attributes set."));
        public static AmbiguousMatchException MultipleVersionsSpecified(string typeName) => new AmbiguousMatchException(Utils.CurrentCulture($"The object of type '{typeName}' could not be read because it has multiple properties with the {nameof(VersionNumberAttribute)} applied."));
        public static ArgumentException InvalidVersionAttribute() => new ArgumentException(Utils.CurrentCulture($"The property with the {nameof(VersionNumberAttribute)} applied must have a single offset supplied and no version restrictions."));
        public static ArgumentException NotValidForStringTypes([CallerMemberName] string methodName = null) => new ArgumentException(Utils.CurrentCulture($"{methodName} cannot be used with strings."));
        public static InvalidCastException PropertyNotConvertable(string propName, string storeType, string propType) => new InvalidCastException(Utils.CurrentCulture($"The property '{propName}' has a {nameof(StoreTypeAttribute)} value of '{storeType}' but '{storeType}' could not be converted to/from '{propType}'."));
        public static ArgumentOutOfRangeException OutOfStreamBounds(object paramValue, [CallerArgumentExpression(nameof(paramValue))] string paramName = null) => new ArgumentOutOfRangeException(paramName, paramValue, Utils.CurrentCulture($"The {paramName} value is out of bounds. The value must be non-negative and no greater than the length of the underlying stream."));

        #endregion
    }
}
