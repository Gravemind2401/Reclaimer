using Reclaimer.IO;

namespace Reclaimer.Geometry
{
    public interface IBufferableVector<out TSelf> : IVector, IBufferable<TSelf>
    { }
}
