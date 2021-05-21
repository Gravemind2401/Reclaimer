using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Annotations
{
    /// <summary>
    /// Specifies the default value for a property should be retrieved from a static source.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class RuntimeDefaultValueAttribute : DefaultValueAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> where the default value is stored.</param>
        /// <param name="propertyName">The name of the static property that contains the default value.</param>
        public RuntimeDefaultValueAttribute(Type type, string propertyName)
            : base(GetValue(type, propertyName))
        { }

        private static object GetValue(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property == null)
                throw new InvalidOperationException($"Property \"{type.Name}.{propertyName}\" is not static or does not exist.");

            var getter = property.GetGetMethod(true);
            if (getter == null)
                throw new InvalidOperationException($"Property \"{type.Name}.{propertyName}\" does not have a get method.");

            var result = property.GetValue(null);
            return result;
        }
    }
}
