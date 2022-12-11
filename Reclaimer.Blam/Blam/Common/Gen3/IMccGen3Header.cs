using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface IMccGen3Header : IGen3Header
    {
        int StringNamespaceCount { get; set; }
        Pointer StringNamespaceTablePointer { get; set; }
    }
}
