using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Halo5;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class BitmaskValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public ObservableCollection<BitValue> Options { get; }

        public BitmaskValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            Options = new ObservableCollection<BitValue>();
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                switch (FieldDefinition.ValueType)
                {
                    case MetaValueType.Bitmask8:
                        Value = reader.ReadByte();
                        break;

                    case MetaValueType.Bitmask16:
                        Value = reader.ReadInt16();
                        break;

                    case MetaValueType.Bitmask32:
                        Value = reader.ReadInt32();
                        break;

                    default:
                        IsEnabled = false;
                        break;
                }

                if (!Options.Any())
                {
                    var index = 0;
                    foreach (var n in node.GetChildElements())
                    {
                        if (n.Name.ToUpperInvariant() != "ITEM")
                            continue;

                        var label = n.GetStringAttribute("name");
                        Options.Add(new BitValue(this, label, index++));
                    }
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            switch (FieldDefinition.ValueType)
            {
                case MetaValueType.Bitmask8:
                    writer.Write((byte)Value);
                    break;

                case MetaValueType.Bitmask16:
                    writer.Write((short)Value);
                    break;

                case MetaValueType.Bitmask32:
                    writer.Write(Value);
                    break;
            }

            IsDirty = false;
        }

        public override JToken GetJValue() => new JValue(Value);

        private void RefreshOptions()
        {
            foreach (var bit in Options)
                bit.Refresh();
        }

        public class BitValue : BindableBase
        {
            private readonly BitmaskValue parent;
            private readonly NameHelper nameHelper;

            public string Name => nameHelper.Name;
            public string ToolTip => nameHelper.ToolTip;

            public int Index { get; }
            public int Mask { get; }

            private bool isChecked;
            public bool IsChecked
            {
                get => isChecked;
                set
                {
                    if (!SetProperty(ref isChecked, value))
                        return;

                    parent.Value = value
                        ? parent.Value | Mask
                        : parent.Value & ~Mask;

                    parent.RefreshOptions();
                }
            }

            public BitValue(BitmaskValue parent, string name, int index)
            {
                this.parent = parent;
                nameHelper = new NameHelper(name);
                Index = index;
                Mask = (int)Math.Pow(2, Index);
                Refresh();
            }

            public void Refresh()
            {
                isChecked = (parent.Value & Mask) > 0;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }
    }
}
