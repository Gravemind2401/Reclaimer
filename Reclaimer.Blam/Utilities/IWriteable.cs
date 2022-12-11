using Reclaimer.IO;

namespace Reclaimer.Blam.Utilities
{
    public interface IWriteable
    {
        void Write(EndianWriter writer, double? version);
    }
}
