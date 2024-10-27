﻿namespace Reclaimer.Drawing
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
