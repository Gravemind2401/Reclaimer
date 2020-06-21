using Adjutant.Blam.Common;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
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
        public ObservableCollection<MetaValueBase> Children { get; }

        public StructureValue(XmlNode node, ICacheFile cache, EndianReader reader, long baseAddress)
            : base(node, cache, reader, baseAddress)
        {
            BlockSize = node.GetIntAttribute("entrySize", "size") ?? 0;
            Children = new ObservableCollection<MetaValueBase>();
            IsExpanded = true;
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                Children.Clear();
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                BlockCount = reader.ReadInt32();
                BlockAddress = new Pointer(reader.ReadInt32(), cache.DefaultAddressTranslator).Address;

                if (BlockCount <= 0 || BlockAddress + BlockCount * BlockSize > reader.BaseStream.Length)
                {
                    IsEnabled = false;
                    return;
                }

                blockIndex = 0;
                foreach (XmlNode n in node.ChildNodes)
                    Children.Add(GetMetaValue(n, cache, BlockAddress));

                RaisePropertyChanged(nameof(BlockIndex));
                RaisePropertyChanged(nameof(HasChildren));

                var entryOffset = node.GetIntAttribute("entryName", "entryOffset", "label");
                var isExplicit = entryOffset.HasValue;
                entryOffset = entryOffset ?? 0;

                var entry = Children.FirstOrDefault(c => c.Offset == entryOffset);
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

            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
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
