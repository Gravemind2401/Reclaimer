namespace Reclaimer.Saber3D.Common
{
    public interface IPakFile
    {
        string FileName { get; }
        IReadOnlyList<IPakItem> Items { get; }
    }
}