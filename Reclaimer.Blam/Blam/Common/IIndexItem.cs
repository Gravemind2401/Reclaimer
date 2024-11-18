using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common
{
    public interface IIndexItem
    {
        ICacheFile CacheFile { get; }

        int Id { get; }
        int ClassId { get; }
        Pointer MetaPointer { get; }
        string TagName { get; }
        string ClassCode { get; }
        string ClassName { get; }

        T ReadMetadata<T>();

        IAddressTranslator GetAddressTranslator() => CacheFile.DefaultAddressTranslator;
        long GetBaseAddress() => MetaPointer.Address;

        sealed string FileName => Utils.GetFileName(TagName);
    }
}
