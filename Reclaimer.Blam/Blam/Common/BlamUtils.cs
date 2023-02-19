using System.Text.RegularExpressions;

namespace Reclaimer.Blam.Common
{
    internal static class BlamUtils
    {
        public static IEnumerable<IGrouping<string, TInstance>> GroupGeometryInstances<TInstance>(IEnumerable<TInstance> instances, Func<TInstance, string> nameSelector)
        {
            return instances.GroupBy(i => Regex.Replace(nameSelector(i), "^[^a-z]+|(?:_[a-z])?[^a-z]+$", string.Empty, RegexOptions.IgnoreCase)).OrderBy(g => g.Key);
        }
    }
}
