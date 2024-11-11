using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class StructureValue : MetaValue, IExpandable
    {
        public int BlockSize { get; }

        private int blockCount;
        public int BlockCount
        {
            get => blockCount;
            set => SetProperty(ref blockCount, value);
        }

        private long blockAddress;
        public long BlockAddress
        {
            get => blockAddress;
            set => SetProperty(ref blockAddress, value);
        }

        private int blockIndex;
        public int BlockIndex
        {
            get => blockIndex;
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
            get => blockLabels;
            set => SetProperty(ref blockLabels, value);
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public bool HasChildren => Children.Any();
        public ObservableCollection<MetaValue> Children { get; }

        IEnumerable<MetaValueBase> IExpandable.Children => Children;

        public StructureValue(XmlNode node, IModuleItem item, IMetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            BlockSize = node.GetChildElements().Sum(n => FieldDefinition.GetHalo5Definition(item, n).Size);
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

                var adjustedOffset = (BaseAddress - Host.Offset) + Offset;
                var structdef = Header.StructureDefinitions.First(s => s.FieldBlock == Host.Index && s.FieldOffset == adjustedOffset);

                var block = Header.DataBlocks[structdef.TargetIndex];
                BlockAddress = Header.GetSectionOffset(block.Section) + block.Offset - Header.HeaderSize;

                blockIndex = 0;
                var offset = 0;
                foreach (var n in node.GetChildElements())
                {
                    var def = FieldDefinition.GetHalo5Definition(Item, n);
                    Children.Add(GetMetaValue(n, Item, Header, block, reader, BlockAddress, offset));
                    offset += def.Size;
                }

                RaisePropertyChanged(nameof(BlockIndex));
                RaisePropertyChanged(nameof(HasChildren));

                var entry = Children.FirstOrDefault(c => c.IsBlockName);
                var isExplicit = entry != null;

                entry ??= Children.First();
                if ((isExplicit && entry is SimpleValue) || entry is StringValue || entry is TagReferenceValue)
                {
                    var labels = new List<string>();
                    for (var i = BlockCount - 1; i >= 0; i--) //end at 0 so the first entry is displayed when done
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
                else
                    BlockLabels = Enumerable.Range(0, BlockCount).Select(i => $"Block {i:D2}");
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            throw new NotImplementedException();
        }

        public override JToken GetJValue()
        {
            var result = new JArray();
            if (BlockCount <= 0 || !HasChildren)
                return result;

            for (var i = 0; i < BlockCount; i++)
            {
                BlockIndex = i;
                var obj = new JObject();

                foreach (var item in Children.Where(c => c.FieldDefinition.ValueType != MetaValueType.Comment))
                {
                    var propName = obj.ContainsKey(item.Name) ? $"{item.Name}_{item.Offset}" : item.Name;
                    obj.Add(propName, item.GetJValue());
                }

                result.Add(obj);
            }

            return result;
        }

        private void RefreshChildren()
        {
            if (BlockCount <= 0)
                return;

            using (var itemReader = Item.CreateReader())
            using (var reader = itemReader.CreateVirtualReader(Header.HeaderSize))
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
