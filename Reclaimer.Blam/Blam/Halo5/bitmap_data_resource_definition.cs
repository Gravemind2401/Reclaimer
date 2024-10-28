using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class bitmap_data_resource_definition
    {
        [Offset(28)]
        public int HardwareFormat { get; set; }

        [Offset(32)]
        public byte TileMode { get; set; }

        [Offset(33)]
        public byte Flags { get; set; }
    }
}
