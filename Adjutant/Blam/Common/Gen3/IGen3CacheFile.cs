using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Common.Gen3
{
    public interface IGen3CacheFile : ICacheFile
    {
        IGen3Header Header { get; }
        ILocaleIndex LocaleIndex { get; }
        bool UsesStringEncryption { get; }
    }
}
