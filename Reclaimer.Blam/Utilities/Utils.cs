using System.Globalization;

namespace Reclaimer.Blam.Utilities
{
    internal static class Utils
    {
        public static string CurrentCulture(FormattableString formattable)
        {
            return formattable?.ToString(CultureInfo.CurrentCulture)
                ?? throw new ArgumentNullException(nameof(formattable));
        }

        public static string GetFileName(this string value) => value?.Split('\\').Last();

        public static TSource? FirstOrNull<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate) where TSource : struct
        {
            foreach (var item in source)
            {
                if (predicate(item))
                    return item;
            }

            return null;
        }

        public static IEnumerable<KeyValuePair<TEnum, TAttribute>> GetEnumAttributes<TEnum, TAttribute>() where TEnum : struct where TAttribute : Attribute
        {
            foreach (var fi in typeof(TEnum).GetFields().Where(f => f.FieldType == typeof(TEnum)))
            {
                var field = (TEnum)fi.GetValue(null);
                foreach (var attr in fi.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>())
                    yield return new KeyValuePair<TEnum, TAttribute>(field, attr);
            }
        }

        public static IEnumerable<TAttribute> GetEnumAttributes<TEnum, TAttribute>(TEnum enumValue) where TEnum : struct where TAttribute : Attribute
        {
            foreach (var fi in typeof(TEnum).GetFields().Where(f => f.FieldType == typeof(TEnum)))
            {
                var field = (TEnum)fi.GetValue(null);
                if (!field.Equals(enumValue))
                    continue;

                foreach (var attr in fi.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>())
                    yield return attr;
            }
        }
    }
}
