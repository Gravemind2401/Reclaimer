using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
