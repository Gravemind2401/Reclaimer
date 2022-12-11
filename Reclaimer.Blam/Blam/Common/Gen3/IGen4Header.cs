using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen3
{
    public interface IGen4Header : IGen3Header
    {
        int UnknownTableSize { get; set; }
        Pointer UnknownTablePointer { get; set; }
    }
}
