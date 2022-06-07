using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO.Dynamic
{
    internal static class Extensions
    {
        public static bool IsVersioned(this IVersionAttribute attr) => attr.HasMinVersion || attr.HasMaxVersion;
        public static bool NotVersioned(this IVersionAttribute attr) => !attr.HasMinVersion && !attr.HasMaxVersion;

        public static bool AllNotEmpty<T>(this IEnumerable<T> source, Func<T, bool> predicate) => source.Any() && source.All(predicate);

        public static bool ValidateOverlap(this IEnumerable<IVersionAttribute> attributes)
        {
            if (!attributes.Any())
                return true;

            if (attributes.Count(NotVersioned) > 1)
                return false;

            var boundaries = attributes.Where(IsVersioned)
                .Select(a => new
                {
                    Attribute = a,
                    Min = a.HasMinVersion ? a.MinVersion : default(double?),
                    Max = a.HasMaxVersion ? a.MaxVersion : default(double?),
                });

            if ((from a in boundaries
                 from b in boundaries.Where(x => x.Attribute != a.Attribute)
                 where (a.Min >= b.Min && a.Min < b.Max) //a.Min is inside b
                 || (a.Max > b.Min && a.Max <= b.Max) //a.Max is inside b
                 || (b.Min >= a.Min && b.Min < a.Max) //b is fully inside a (if both above are false)
                 select 1).Any())
                return false;

            return true;
        }
    }
}
