using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds.Annotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class XboxDecompressorAttribute : Attribute, IFormatAttribute<XboxFormat>
    {
        public XboxFormat Format { get; }

        public XboxDecompressorAttribute(XboxFormat format)
        {
            Format = format;
        }
    }
}
