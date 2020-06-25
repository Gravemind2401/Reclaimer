using Adjutant.Blam.Halo5;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class StructureValue : MetaValue, IExpandable
    {
        public int BlockSize { get; }

        private int blockCount;
        public int BlockCount
        {
            get { return blockCount; }
            set { SetProperty(ref blockCount, value); }
        }

        private long blockAddress;
        public long BlockAddress
        {
            get { return blockAddress; }
            set { SetProperty(ref blockAddress, value); }
        }

        private int blockIndex;
        public int BlockIndex
        {
            get { return blockIndex; }
            set
            {
                value = Math.Min(Math.Max(0, value), BlockCount - 1);

                if (SetProperty(ref blockIndex, value))
                    RefreshChildren();
            }
        }

        private IEnumerable<string> blockLabels;
        public IEnumerable<string> BlockLabels
        {
            get { return blockLabels; }
            set { SetProperty(ref blockLabels, value); }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        public bool HasChildren => Children.Any();
        public ObservableCollection<MetaValue> Children { get; }

        IEnumerable<MetaValueBase> IExpandable.Children => Children;

        public StructureValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            BlockSize = node.ChildNodes.OfType<XmlNode>().Sum(n => FieldDefinition.GetHalo5Definition(n).Size);
            Children = new ObservableCollection<MetaValue>();
            IsExpanded = true;
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                Children.Clear();
                reader.Seek(ValueAddress + 16, SeekOrigin.Begin);

                BlockCount = reader.ReadInt32();

                if (BlockCount <= 0 || BlockAddress + BlockCount * BlockSize > reader.BaseStream.Length)
                {
                    IsEnabled = false;
                    return;
                }

                var adjustedOffset = (BaseAddress - host.Offset) + Offset;
                var structdef = header.StructureDefinitions.First(s => s.FieldBlock == header.DataBlocks.IndexOf(host) && s.FieldOffset == adjustedOffset);

                var block = header.DataBlocks[structdef.TargetIndex];
                BlockAddress = header.GetSectionOffset(block.Section) + block.Offset - header.Header.HeaderSize;

                blockIndex = 0;
                var offset = 0;
                foreach (XmlNode n in node.ChildNodes)
                {
                    var def = FieldDefinition.GetHalo5Definition(n);
                    Children.Add(GetMetaValue(n, item, header, block, reader, BlockAddress, offset));
                    offset += def.Size;
                }

                RaisePropertyChanged(nameof(BlockIndex));
                RaisePropertyChanged(nameof(HasChildren));

                var entry = Children.FirstOrDefault(c => c.IsBlockName);
                var isExplicit = entry != null;

                entry = entry ?? Children.First();
                if ((isExplicit && entry is SimpleValue) || entry is StringValue || entry is TagReferenceValue)
                {
                    var labels = new List<string>();
                    for (int i = BlockCount - 1; i >= 0; i--) //end at 0 so the first entry is displayed when done
                    {
                        entry.BaseAddress = BlockAddress + i * BlockSize;
                        entry.ReadValue(reader);

                        if (entry.EntryString == null)
                            labels.Insert(0, $"Block {i:D2}");
                        else
                            labels.Insert(0, $"{i:D2} : {entry.EntryString}");
                    }
                    BlockLabels = labels;
                }
                else BlockLabels = Enumerable.Range(0, BlockCount).Select(i => $"Block {i:D2}");
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            throw new NotImplementedException();
        }

        private void RefreshChildren()
        {
            if (BlockCount <= 0)
                return;

            using (var itemReader = item.CreateReader())
            using (var reader = itemReader.CreateVirtualReader(header.Header.HeaderSize))
            {
                foreach (var c in Children)
                {
                    c.BaseAddress = BlockAddress + BlockIndex * BlockSize;
                    c.ReadValue(reader);
                }
            }
        }
    }
}
