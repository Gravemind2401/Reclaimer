using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2
{
    public class scenario
    {
        [Offset(528)]
        public BlockCollection<StructureBspBlock> StructureBSPs { get; set; }
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

        [Offset(20)]
        public TagReference BSPReference { get; set; }
    }
}
