using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common
{
    public interface ICacheFile
    {
        ByteOrder ByteOrder { get; }
        string FileName { get; }
        string BuildString { get; }
        CacheType CacheType { get; }

        ITagIndex<IIndexItem> TagIndex { get; }
        IStringIndex StringIndex { get; }

        IAddressTranslator DefaultAddressTranslator { get; }
    }
}
