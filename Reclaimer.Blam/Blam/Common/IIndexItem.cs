using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common
{
    public interface IIndexItem
    {
        ICacheFile CacheFile { get; }

        int Id { get; }
        int ClassId { get; }
        Pointer MetaPointer { get; }
        string FullPath { get; }
        string ClassCode { get; }
        string ClassName { get; }

        T ReadMetadata<T>();

        sealed string FileName => Utilities.Utils.GetFileName(FullPath);
    }
}
