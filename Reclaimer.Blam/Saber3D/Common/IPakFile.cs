using Reclaimer.Blam.Utilities;

namespace Reclaimer.Saber3D.Common
{
    public interface IPakFile
    {
        bool IsMcc { get; }
        string FileName { get; }
        IReadOnlyList<IPakItem> Items { get; }
        IPakItem FindItem(PakItemType itemType, string name, bool external);
        DependencyReader CreateReader();
    }
}