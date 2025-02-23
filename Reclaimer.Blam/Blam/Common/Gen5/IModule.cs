using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen5
{
    public interface IModule
    {
        string FileName { get; }
        ModuleType ModuleType { get; }

        void AddLinkedModule(string fileName);
        IEnumerable<IModuleItem> FindAlternateTagInstances(int globalTagId);
        IEnumerable<TagClass> GetTagClasses();

        IModuleItem GetItemById(int globalTagId);
        IEnumerable<IModuleItem> GetItemsByClass(string classCode);
        IEnumerable<IModuleItem> GetLinkedItems();

        DependencyReader CreateReader();

        sealed IEnumerable<IModuleItem> EnumerateItems() => GetTagClasses().SelectMany(c => GetItemsByClass(c.ClassCode));
    }
}