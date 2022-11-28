using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer
{
    public static class Extensions
    {
        public static IEnumerable<T> GetRange<T>(this IReadOnlyList<T> source, Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(source.Count);
            return GetSubset(source, offset, length);
        }

        public static IEnumerable<T> GetSubset<T>(this IReadOnlyList<T> source, int offset, int count)
        {
            if (offset < 0 || offset >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(offset));

            var end = offset + count;
            if (end < 0 || end > source.Count)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (var i = offset; i < end; i++)
                yield return source[i];
        }
    }
}
