using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer
{
    internal static class Extensions
    {
        public static IEnumerable<T> Subset<T>(this IReadOnlyList<T> source, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(source.Count);
            return Subset(source, offset, length);
        }

        public static IEnumerable<T> Subset<T>(this IReadOnlyList<T> source, int index, int length)
        {
            if (index < 0 || index >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var end = index + length;
            if (end < 0 || end > source.Count)
                throw new ArgumentOutOfRangeException(nameof(length));

            for (var i = index; i < end; i++)
                yield return source[i];
        }
    }
}
