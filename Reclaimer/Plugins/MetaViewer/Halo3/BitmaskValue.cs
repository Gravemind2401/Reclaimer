using Prism.Mvvm;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
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

        public BitmaskValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            Options = new ObservableCollection<BitValue>();
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
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
                    foreach (var n in node.GetChildElements())
                    {
                        if (n.Name.ToUpper() != "OPTION" && n.Name.ToUpper() != "BIT")
                            continue;

                        var val = n.GetIntAttribute("index", "value");
                        var label = n.GetStringAttribute("name");

                        if (val >= 0)
                            Options.Add(new BitValue(this, label, val.Value));
                    }
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
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

        private void RefreshOptions()
        {
            foreach (var bit in Options)
                bit.Refresh();
        }

        public class BitValue : BindableBase
        {
            private readonly BitmaskValue parent;

            public string Name { get; }
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
                Name = name;
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
