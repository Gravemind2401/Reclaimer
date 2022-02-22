using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds.Annotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class DxgiDecompressorAttribute : Attribute, IFormatAttribute<DxgiFormat>
    {
        public DxgiFormat Format { get; }

        public DxgiDecompressorAttribute(DxgiFormat format)
        {
            Format = format;
        }
    }
}
