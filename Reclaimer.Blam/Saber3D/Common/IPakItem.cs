namespace Reclaimer.Saber3D.Common
{
    public interface IPakItem
    {
        int Address { get; }
        IPakFile Container { get; }
        PakItemType ItemType { get; }
        string Name { get; }
        int Size { get; }
    }
}