using System;
using System.Collections.Generic;
using System.Linq;

namespace Reclaimer.IO.Dynamic
{
    internal static class Extensions
    {
        public static bool SupportsNullVersion<T>(this IEnumerable<T> source) where T : IVersionAttribute
        {
            return !source.Any() || source.Any(a => !a.IsVersioned);
        }

        public static bool HasVersion<T>(this IEnumerable<T> source, double? version) where T : IVersionAttribute
        {
            return source.Any(a => ValidateVersion(a, version));
        }

        public static T GetVersion<T>(this IEnumerable<T> source, double? version) where T : IVersionAttribute
        {
            return source.OrderByDescending(a => a.IsVersioned)
                .FirstOrDefault(a => ValidateVersion(a, version));
        }

        public static bool ValidateVersion(this IVersionAttribute attr, double? version) => ValidateVersion(version, attr.HasMinVersion ? attr.MinVersion : null, attr.HasMaxVersion ? attr.MaxVersion : null);

        public static bool ValidateVersion(double? version, double? min, double? max)
        {
            return (version >= min || !min.HasValue) && (version < max || !max.HasValue || max == min);
        }

        public static bool ValidateOverlap(this IEnumerable<IVersionAttribute> source)
        {
            if (!source.Any())
                return true;

            if (source.Count(a => !a.IsVersioned) > 1)
                return false;

            var boundaries = source.Where(a => a.IsVersioned)
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
