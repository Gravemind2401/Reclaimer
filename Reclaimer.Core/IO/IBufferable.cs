using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public interface IBufferable<out TBufferable>
    {
        static int PackSize { get; } //TODO: abstract static in C# 10
        static int SizeOf { get; } //TODO: abstract static in C# 10
        static TBufferable ReadFromBuffer(ReadOnlySpan<byte> buffer) => throw new NotImplementedException(); //TODO: abstract static in C# 10
        void WriteToBuffer(Span<byte> buffer);
    }
}
