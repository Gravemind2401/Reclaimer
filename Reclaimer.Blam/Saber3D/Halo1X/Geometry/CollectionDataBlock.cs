using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Saber3D.Halo1X.Geometry
{
    public abstract class CollectionDataBlock : DataBlock
    {
        public List<DataBlock> ChildBlocks { get; } = new List<DataBlock>();

        protected TBlock GetUniqueChild<TBlock>() => ChildBlocks.OfType<TBlock>().Single();
        protected TBlock GetOptionalChild<TBlock>() => ChildBlocks.OfType<TBlock>().SingleOrDefault();
        protected IEnumerable<TBlock> FilterChildren<TBlock>() => ChildBlocks.OfType<TBlock>();

        internal override void Read(EndianReader reader) => ReadChildren(reader);

        protected void ReadChildren(EndianReader reader)
        {
            while (reader.Position < Header.EndOfBlock)
                ChildBlocks.Add(reader.ReadBlock(Owner, this));
        }

        protected void ReadChildren(EndianReader reader, int assertCount)
        {
            ReadChildren(reader);

            if (ChildBlocks.Count != assertCount)
                Debugger.Break();
        }

        protected void PopulateChildrenOfType<TBlock>(List<TBlock> list) => list.AddRange(ChildBlocks.OfType<TBlock>());

        protected void PopulateGroupList<TGroup>(List<TGroup> list, int assertCount)
            where TGroup : DataBlockGroup, new()
        {
            var nextGroup = new TGroup();
            foreach (var block in ChildBlocks)
            {
                if (block is EmptyBlock)
                {
                    list.Add(nextGroup);
                    nextGroup = new TGroup();
                }
                else
                    nextGroup.Add(block);
            }

            if (list.Count != assertCount)
                Debugger.Break();
        }

        internal override void Validate()
        {
            if (ExpectedChildCount >= 0 && ChildBlocks.Count > ExpectedChildCount)
                Debugger.Break();
        }

        protected override object GetDebugProperties() => new { ChildCount = ChildBlocks.Count, Header.StartOfBlock };
    }
}
