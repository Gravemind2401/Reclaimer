using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo2
{
    public class scenario
    {
        [Offset(528)]
        public BlockCollection<StructureBspBlock> StructureBsps { get; set; }
    }

    [FixedSize(68)]
    public class StructureBspBlock
    {
        [Offset(0)]
        public int MetadataAddress { get; set; }

        [Offset(4)]
        public int Size { get; set; }

        [Offset(8)]
        public int Magic { get; set; }

        [Offset(16)]
        public TagReference BspReference { get; set; }
    }
}
