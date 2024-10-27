namespace Reclaimer.Drawing
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
