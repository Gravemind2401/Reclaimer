using Reclaimer.Blam.Common;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Saber3D.Common;
using System.Text.RegularExpressions;

namespace Conduit
{
    internal static class Common
    {
        public static IEnumerable<IIndexItem> EnumerateTags(this ICacheFile cache, string filter, string sort, int limit)
        {
            var tags = cache.TagIndex.AsEnumerable();

            if (!string.IsNullOrEmpty(filter))
            {
                var pattern = MakeFilterPattern(filter);
                tags = tags.Where(t =>
                {
                    var shortClass = $"{t.TagName}.{t.ClassCode}";
                    var longClass = $"{t.TagName}.{t.ClassName}";
                    return pattern.IsMatch(shortClass) || pattern.IsMatch(longClass);
                });
            }

            if (!string.IsNullOrEmpty(sort))
                tags = tags.OrderBy(t => MakeOutputString(t, sort));

            if (limit > 0)
                tags = tags.Take(limit);

            return tags;
        }

        public static IEnumerable<IModuleItem> EnumerateTags(this Reclaimer.Blam.Common.Gen5.IModule module, string filter, string sort, int limit)
        {
            var tags = module.EnumerateItems();

            if (!string.IsNullOrEmpty(filter))
            {
                var pattern = MakeFilterPattern(filter);
                tags = tags.Where(t =>
                {
                    var shortClass = $"{t.TagName}.{t.ClassCode}";
                    var longClass = $"{t.TagName}.{t.ClassName}";
                    return pattern.IsMatch(shortClass) || pattern.IsMatch(longClass);
                });
            }

            if (!string.IsNullOrEmpty(sort))
                tags = tags.OrderBy(t => MakeOutputString(t, sort));

            if (limit > 0)
                tags = tags.Take(limit);

            return tags;
        }

        public static IEnumerable<IPakItem> EnumerateTags(this IPakFile pak, string filter, string sort, int limit)
        {
            var tags = pak.Items.AsEnumerable();

            if (!string.IsNullOrEmpty(filter))
            {
                var pattern = MakeFilterPattern(filter);
                tags = tags.Where(t => pattern.IsMatch($"{t.Name}.{t.ItemType}"));
            }

            if (!string.IsNullOrEmpty(sort))
                tags = tags.OrderBy(t => MakeOutputString(t, sort));

            if (limit > 0)
                tags = tags.Take(limit);

            return tags;
        }

        public static Regex MakeFilterPattern(string filter) => new Regex(Regex.Escape(filter).Replace("?", ".").Replace("*", ".*"), RegexOptions.IgnoreCase);

        public static string MakeOutputString(this IIndexItem tag, string format)
        {
            return format
                .Replace("%i", tag.Id.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("%t", tag.TagName, StringComparison.OrdinalIgnoreCase)
                .Replace("%n", tag.FileName, StringComparison.OrdinalIgnoreCase)
                .Replace("%e", tag.ClassName, StringComparison.OrdinalIgnoreCase)
                .Replace("%c", tag.ClassCode, StringComparison.OrdinalIgnoreCase);
        }

        public static string MakeOutputString(this IModuleItem tag, string format)
        {
            return format
                .Replace("%i", tag.GlobalTagId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("%t", tag.TagName, StringComparison.OrdinalIgnoreCase)
                .Replace("%n", tag.FileName, StringComparison.OrdinalIgnoreCase)
                .Replace("%e", tag.ClassName, StringComparison.OrdinalIgnoreCase)
                .Replace("%c", tag.ClassCode, StringComparison.OrdinalIgnoreCase);
        }

        public static string MakeOutputString(this IPakItem tag, string format)
        {
            return format
                .Replace("%i", tag.Address.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("%t", tag.Name, StringComparison.OrdinalIgnoreCase)
                .Replace("%n", tag.Name, StringComparison.OrdinalIgnoreCase)
                .Replace("%e", tag.ItemType.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("%c", tag.ItemType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
