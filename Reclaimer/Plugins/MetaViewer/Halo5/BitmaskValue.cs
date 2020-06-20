using Adjutant.Blam.Halo5;
using Prism.Mvvm;
using Reclaimer.Utilities;
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
    public class BitmaskValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetMetaProperty(ref _value, value); }
        }

        public ObservableCollection<BitValue> Options { get; }

        public BitmaskValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, reader, baseAddress, offset)
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

                switch (ValueType)
                {
                    case MetaValueType._field_byte_flags:
                        Value = reader.ReadByte();
                        break;

                    case MetaValueType._field_word_flags:
                        Value = reader.ReadInt16();
                        break;

                    case MetaValueType._field_long_flags:
                    case MetaValueType._field_long_block_flags:
                        Value = reader.ReadInt32();
                        break;

                    default:
                        IsEnabled = false;
                        break;
                }

                if (!Options.Any())
                {
                    int index = 0;
                    foreach (XmlNode n in node.ChildNodes)
                    {
                        if (n.Name.ToUpperInvariant() != "ITEM")
                            continue;

                        var label = GetStringAttribute(n, "name");
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

            switch (ValueType)
            {
                case MetaValueType._field_byte_flags:
                    writer.Write((byte)Value);
                    break;

                case MetaValueType._field_word_flags:
                    writer.Write((short)Value);
                    break;

                case MetaValueType._field_long_flags:
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
                get { return isChecked; }
                set
                {
                    if (!SetProperty(ref isChecked, value))
                        return;

                    if (value)
                        parent.Value |= Mask;
                    else parent.Value &= ~Mask;

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
