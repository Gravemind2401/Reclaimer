using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Annotations
{
    [AttributeUsage(AttributeTargets.All)]
    public class RuntimeDefaultValueAttribute : DefaultValueAttribute
    {
        public RuntimeDefaultValueAttribute(Type type, string propertyName)
            : base(GetValue(type, propertyName))
        { }

        private static object GetValue(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var getter = property?.GetGetMethod(true);

            if (getter == null)
                throw new InvalidOperationException($"Property \"{type.Name}.{propertyName}\" does not exist or does not have get method.");

            var result = property.GetValue(null);
            return result;
        }
    }
}
