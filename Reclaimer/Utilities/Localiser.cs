using System.ComponentModel;
using System.Reflection;

namespace Reclaimer.Utilities
{
    public static class Localiser
    {
        //TODO: read custom values from somewhere
        internal static readonly Dictionary<string, Dictionary<string, string>> valueLookup = new Dictionary<string, Dictionary<string, string>>();

        public static void ConfigureResourceType(Type type)
        {
            var props = type.GetProperties()
                .Where(PropertyPredicate)
                .Select(p => new
                {
                    Info = p,
                    Default = p.GetCustomAttribute<DefaultValueAttribute>()?.Value as string ?? p.Name
                }).ToList();

            var userValues = valueLookup.GetValueOrDefault(type.FullName);

            foreach (var prop in props)
            {
                var key = prop.Info.Name;
                var value = userValues?.GetValueOrDefault(key);

                if (string.IsNullOrEmpty(value))
                    value = prop.Default;

                var setter = prop.Info.GetSetMethod(true);
                setter.Invoke(null, new[] { value });
            }
        }

        private static bool PropertyPredicate(PropertyInfo prop)
        {
            if (prop.PropertyType != typeof(string))
                return false;

            var getter = prop.GetGetMethod(true);
            if (getter?.IsStatic != true || getter.Invoke(null, null) != null)
                return false;

            return prop.GetSetMethod(true)?.IsStatic == true;
        }
    }
}
