using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen5
{
    public interface IModuleItem
    {
        IModule Module { get; }

        int GlobalTagId { get; }
        int ClassId { get; }
        long AssetId { get; }

        string TagName { get; }
        string ClassCode { get; }
        string ClassName { get; }

        IEnumerable<IModuleItem> EnumerateResourceItems();

        DependencyReader CreateReader();
        T ReadMetadata<T>();

        sealed string FileName => TagName.GetFileName();
    }
}