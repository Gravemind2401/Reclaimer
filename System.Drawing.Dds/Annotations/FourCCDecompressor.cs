using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds.Annotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class FourCCDecompressorAttribute : Attribute, IFormatAttribute<FourCC>
    {
        public FourCC Format { get; }

        public FourCCDecompressorAttribute(FourCC format)
        {
            Format = format;
        }
    }
}
