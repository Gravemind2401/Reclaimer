using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Utilities
{
    public static class Extensions
    {
        public static int FirstIndexWhere<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            int index = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }

        public static int LastIndexWhere<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            int match = -1;
            int index = 0;

            foreach (var item in source)
                if (predicate(item)) match = index++;

            return match;
        }

        public static IEnumerable<string> SplitPath(this DirectoryInfo directory)
        {
            while (directory != null)
            {
                yield return directory.Name;
                directory = directory.Parent;
            }
        }

        /// <summary>
        /// Performs a case-insensitive replace.
        /// </summary>
        /// <param name="input">The string to search.</param>
        /// <param name="pattern">The value that will be replaced.</param>
        /// <param name="replacement">The replacement value.</param>
        /// <returns></returns>
        public static string PatternReplace(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, Regex.Escape(pattern), replacement, RegexOptions.IgnoreCase);
        }

        public static void TransitionTo(this Window prev, Window next)
        {
            //if we try to close window A then show and drag window B we get the error 'Can only call DragMove when primary mouse button is down.'
            //this is a workaround to ensure window A has closed before window B attempts to show and drag
            prev.Closed += (s, e) =>
            {
                next.Show();
                next.DragMove();
            };
            prev.Close();
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            return dic.ContainsKey(key) ? dic[key] : default(TValue);
        }
    }
}
