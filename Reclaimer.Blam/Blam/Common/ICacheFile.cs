using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.Common
{
    public interface ICacheFile
    {
        ByteOrder ByteOrder { get; }
        string FileName { get; }
        string BuildString { get; }
        CacheType CacheType { get; }
        CacheMetadata Metadata { get; }

        ITagIndex<IIndexItem> TagIndex { get; }
        IStringIndex StringIndex { get; }

        IAddressTranslator DefaultAddressTranslator { get; }
    }
}
