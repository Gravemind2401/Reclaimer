using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Annotations
{
    /// <summary>
    /// Specifies that a plugin function should be made available for use by other plugins.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SharedFunctionAttribute : Attribute
    {
        /// <summary>
        /// The name of the shared function. If not specified, the source function name is used.
        /// </summary>
        public string Name { get; set; }
    }
}
