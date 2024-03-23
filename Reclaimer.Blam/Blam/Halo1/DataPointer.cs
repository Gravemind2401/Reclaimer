using Reclaimer.Blam.Utilities;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    [FixedSize(20)]
    public struct DataPointer
    {
        public readonly long Address => Pointer.Address;

        [Offset(0)]
        public int DataLength { get; set; }

        [Offset(4)]
        public int Unknown1 { get; set; }

        [Offset(8)]
        public int Unknown2 { get; set; }

        [Offset(12)]
        public Pointer Pointer { get; set; }

        [Offset(16)]
        public int Unknown3 { get; set; }
    }
}
