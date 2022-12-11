using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface IMccCacheFile : IGen3CacheFile
    {
        IPointerExpander PointerExpander { get; }
    }
}
