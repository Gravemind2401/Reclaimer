using System.IO;

namespace Reclaimer.IO
{
    internal interface IEndianStream
    {
        ByteOrder ByteOrder { get; }
        long Position { get; }
        void Seek(long offset, SeekOrigin origin);
    }
}
