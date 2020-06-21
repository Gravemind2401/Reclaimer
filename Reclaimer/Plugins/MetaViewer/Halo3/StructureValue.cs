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
    public class StructureValue : MetaValue
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

        public StructureValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            BlockSize = node.GetIntAttribute("entrySize", "size") ?? 0;
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
                    Children.Add(MetaValue.GetValue(n, cache, BlockAddress));

                RaisePropertyChanged(nameof(BlockIndex));
                RaisePropertyChanged(nameof(HasChildren));

                var entryOffset = node.GetIntAttribute("entryName", "entryOffset", "label");
                var entry = Children.FirstOrDefault(c => c.Offset == entryOffset);

                if (entry == null)
                    BlockLabels = Enumerable.Range(0, Math.Min(BlockCount, 100)).Select(i => $"Block {i:D2}");
                else
                {
                    var labels = new List<string>();
                    for (int i = 0; i < BlockCount; i++)
                    {
                        entry.BaseAddress = BlockAddress + i * BlockSize;
                        entry.ReadValue(reader);
                        labels.Add($"{i:D2} : {entry.EntryString}");
                    }
                    BlockLabels = labels;
                }
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
