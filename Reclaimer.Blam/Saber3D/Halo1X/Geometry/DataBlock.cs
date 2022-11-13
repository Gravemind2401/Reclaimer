using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class DataBlock
    {
        protected string TypeName => GetType() == typeof(DataBlock) ? "Unmapped" : GetType().Name.Replace("Block", "");

        internal virtual int ExpectedSize => BlockExtensions.AttributeLookup.GetValueOrDefault(GetType())?.ExpectedSize ?? -1;
        internal virtual int ExpectedChildCount => BlockExtensions.AttributeLookup.GetValueOrDefault(GetType())?.ExpectedChildCount ?? -1;

        public int BlockSize => EndOfBlock - (Origin + 6);
        public string TypeString => $"0x{BlockType:X4}";

        public int Origin { get; set; }
        public ushort BlockType { get; set; }
        public int EndOfBlock { get; set; }

        internal virtual void Read(EndianReader reader)
        {
            reader.ReadObject((object)this);
        }

        protected void EndRead(long position)
        {
            if (position != EndOfBlock)
                Debugger.Break();
        }

        internal virtual void Validate() { }

        protected virtual string GetDebuggerDisplay() => new { Type = $"[{BlockType:X4}] {TypeName}", Size = BlockSize, Origin }.ToString();
    }
}
