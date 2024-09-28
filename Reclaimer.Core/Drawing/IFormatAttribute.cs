namespace Reclaimer.Drawing
{
    internal interface IFormatAttribute<T>
    {
        T Format { get; }
    }
}
