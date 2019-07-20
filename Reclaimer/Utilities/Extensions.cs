using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
