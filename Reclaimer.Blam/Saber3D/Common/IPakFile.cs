namespace Reclaimer.Saber3D.Common
{
    public interface IPakFile
    {
        string FileName { get; }
        IReadOnlyList<IPakItem> Items { get; }
        IPakItem FindItem(PakItemType itemType, string name, bool external);
    }
}