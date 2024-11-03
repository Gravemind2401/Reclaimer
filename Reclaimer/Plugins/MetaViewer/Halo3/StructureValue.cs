using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common.Gen3;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class StructureValue : MetaValue, IExpandable
    {
        private MetaValue labelValue;

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

        public ObservableCollection<string> BlockLabels { get; }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        public bool HasChildren => Children.Any();
        public DeepObservableCollection<MetaValue> Children { get; }

        IEnumerable<MetaValueBase> IExpandable.Children => Children;

        public StructureValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            BlockSize = node.GetIntAttribute("elementSize", "entrySize", "size") ?? 0;
            Children = new DeepObservableCollection<MetaValue>();
            BlockLabels = new ObservableCollection<string>();
            IsExpanded = true;

            ReadValue(reader);

            Children.ChildPropertyChanged += Children_ChildPropertyChanged;
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                //force block index dropdown to reset
                blockIndex = -1;
                RaisePropertyChanged(nameof(BlockIndex));

                Children.Clear();
                BlockLabels.Clear();
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                var expander = (context.Cache as IMccCacheFile)?.PointerExpander;
                BlockCount = reader.ReadInt32();
                BlockAddress = new Pointer(reader.ReadInt32(), context.Cache.DefaultAddressTranslator, expander).Address;

                if (BlockCount <= 0 || BlockAddress < 0 || BlockAddress + BlockCount * BlockSize > reader.BaseStream.Length)
                {
                    IsEnabled = false;
                    return;
                }

                blockIndex = 0;
                foreach (var n in node.GetChildElements())
                    Children.Add(GetMetaValue(n, context, BlockAddress));

                var entryOffset = node.GetIntAttribute("entryName", "entryOffset", "label");
                var isExplicit = entryOffset.HasValue;
                entryOffset ??= 0;

                labelValue = Children.FirstOrDefault(c => c.Offset == entryOffset);
                if ((isExplicit && labelValue is SimpleValue) || labelValue is StringValue || labelValue is TagReferenceValue)
                {
                    var labels = new List<string>();
                    for (var i = BlockCount - 1; i >= 0; i--) //end at 0 so the first entry is displayed when done
                    {
                        labelValue.BaseAddress = BlockAddress + i * BlockSize;
                        labelValue.ReadValue(reader);
                        labels.Insert(0, GetEntryString(i, labelValue));
                    }
                    BlockLabels.AddRange(labels);
                }
                else
                {
                    labelValue = null;
                    BlockLabels.AddRange(Enumerable.Range(0, BlockCount).Select(i => $"Block {i:D2}"));
                }

                RaisePropertyChanged(nameof(BlockIndex));
                RaisePropertyChanged(nameof(HasChildren));
            }
            catch { IsEnabled = false; }

            IsBusy = false;
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

        private static string GetEntryString(int index, MetaValue value)
        {
            return value.EntryString == null ? $"Block {index:D2}" : $"{index:D2} : {value.EntryString}";
        }

        private void Children_ChildPropertyChanged(object sender, ChildPropertyChangedEventArgs e)
        {
            if (IsBusy || labelValue == null || e.Element != labelValue)
                return;

            var temp = BlockIndex;
            BlockLabels[BlockIndex] = GetEntryString(BlockIndex, labelValue);
            IsBusy = false;

            //force binding to refresh
            blockIndex = -1;
            RaisePropertyChanged(nameof(BlockIndex));
            blockIndex = temp;
            RaisePropertyChanged(nameof(BlockIndex));
        }

        private void RefreshChildren()
        {
            if (BlockCount <= 0)
                return;

            IsBusy = true;
            using (var reader = context.CreateReader())
            {
                foreach (var c in Children)
                {
                    c.BaseAddress = BlockAddress + BlockIndex * BlockSize;
                    c.ReadValue(reader);
                }
            }
            IsBusy = false;
        }
    }
}
