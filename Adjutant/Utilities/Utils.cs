﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    internal static class Utils
    {
        public static string CurrentCulture(FormattableString formattable)
        {
            if (formattable == null)
                throw new ArgumentNullException(nameof(formattable));

            return formattable.ToString(CultureInfo.CurrentCulture);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        public static string GetFileName(this string value)
        {
            return value?.Split('\\').Last();
        }

        public static TSource? FirstOrNull<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate) where TSource : struct
        {
            foreach (var item in source)
            {
                if (predicate(item))
                    return item;
            }

            return null;
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            return dic.ContainsKey(key) ? dic[key] : default(TValue);
        }
    }
}
