namespace Reclaimer.Drawing.Annotations
{
    internal interface IFormatAttribute<T>
    {
        T Format { get; }
    }
}
