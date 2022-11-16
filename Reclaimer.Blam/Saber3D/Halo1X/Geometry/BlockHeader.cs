using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class BlockHeader
    {
        public int BlockSize => EndOfBlock - (StartOfBlock + 6);
        public string TypeString => $"0x{BlockType:X4}";

        public int StartOfBlock { get; set; }
        public ushort BlockType { get; set; }
        public int EndOfBlock { get; set; }

        private string GetDebuggerDisplay() => TypeString;
    }
}
