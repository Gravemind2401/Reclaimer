using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Reclaimer
{
    internal static class Utils
    {
        public static string CurrentCulture(FormattableString formattable) => formattable?.ToString(CultureInfo.CurrentCulture) ?? throw new ArgumentNullException(nameof(formattable));

        public static T Clamp<T>(in T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            else if (value.CompareTo(max) > 0)
                return max;
            else
                return value;
        }

        public static bool TryConvert(ref object value, Type fromType, Type toType)
        {
            if (value?.GetType() == toType)
                return true;

            if (fromType.IsEnum)
            {
                fromType = fromType.GetEnumUnderlyingType();
                value = Convert.ChangeType(value, fromType);
            }

            if (toType.IsGenericType && toType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nullableType = toType.GetGenericArguments().Single();
                if (TryConvert(ref value, fromType, nullableType))
                    fromType = nullableType;
            }

            var converter = TypeDescriptor.GetConverter(fromType);
            if (converter.CanConvertTo(toType))
            {
                value = converter.ConvertTo(value, toType);
                return true;
            }

            converter = TypeDescriptor.GetConverter(toType);
            if (converter.CanConvertFrom(fromType))
            {
                value = converter.ConvertFrom(value);
                return true;
            }

            return false;
        }

        public static PropertyInfo PropertyFromExpression<TSource, TProperty>(Expression<Func<TSource, TProperty>> expr)
        {
            var type = typeof(TSource);

            if (expr.Body is not MemberExpression member)
                throw new ArgumentException("Expression does not refer to a property");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException("Expression does not refer to a property");

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException("Property does not belong to type");

            return propInfo;
        }

        public static void ReverseEndianness(Span<byte> span, int packSize)
        {
            for (var i = 0; i < span.Length; i += packSize)
                span.Slice(i, packSize).Reverse();
        }
    }
}
