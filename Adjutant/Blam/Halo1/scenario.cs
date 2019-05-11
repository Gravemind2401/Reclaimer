using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo1
{
    public class scenario
    {
        [Offset(1444)]
        public BlockCollection<StructureBspBlock> StructureBSPs { get; set; }
    }

    [FixedSize(32)]
    public class StructureBspBlock
    {
        [Offset(0)]
        public int MetadataAddress { get; set; }

        [Offset(4)]
        public int Size { get; set; }

        [Offset(8)]
        public int Magic { get; set; }

        [Offset(28)]
        public TagReference BSPReference { get; set; }
    }
}
