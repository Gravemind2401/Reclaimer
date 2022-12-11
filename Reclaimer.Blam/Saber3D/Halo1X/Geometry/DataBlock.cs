using Reclaimer.IO;
using System.Diagnostics;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class DataBlock
    {
        internal virtual int ExpectedSize => BlockExtensions.AttributeLookup.GetValueOrDefault(GetType())?.ExpectedSize ?? -1;
        internal virtual int ExpectedChildCount => BlockExtensions.AttributeLookup.GetValueOrDefault(GetType())?.ExpectedChildCount ?? -1;

        protected INodeGraph Owner { get; private set; }
        protected DataBlock ParentBlock { get; private set; }

        public BlockHeader Header { get; } = new BlockHeader();

        internal virtual void Read(EndianReader reader)
        {
            reader.ReadObject((object)this);
        }

        protected void EndRead(long position)
        {
            if (position != Header.EndOfBlock)
                Debugger.Break();
        }

        internal virtual void Validate() { }

        protected virtual object GetDebugProperties() => new { Size = Header.BlockSize, Header.StartOfBlock };

        protected virtual string GetDebuggerDisplay()
        {
            var displayName = GetType() == typeof(DataBlock) ? "Unmapped" : GetType().Name.Replace("Block", "").Replace(Header.TypeString, "");
            if (this is CollectionDataBlock)
                displayName = "*" + displayName;
            return $"[{Header.BlockType:X4}] {displayName} {GetDebugProperties()}";
        }

        internal void SetOwner(INodeGraph owner) => Owner = owner;
        internal void SetParent(DataBlock parent) => ParentBlock = parent;
    }
}
