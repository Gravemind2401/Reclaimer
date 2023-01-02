namespace Reclaimer.Drawing.Annotations
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
