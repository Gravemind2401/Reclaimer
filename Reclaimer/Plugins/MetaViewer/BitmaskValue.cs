using Adjutant.Blam.Common;
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

namespace Reclaimer.Plugins.MetaViewer
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

        public BitmaskValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
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

                Options.Clear();
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.Name.ToUpper() != "OPTION" && n.Name.ToUpper() != "BIT")
                        continue;

                    var val = GetIntAttribute(n, "index");
                    var label = GetStringAttribute(n, "name");

                    if (val >= 0)
                        Options.Add(new BitValue(this, label, val.Value));
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
                case MetaValueType.Enum8:
                    writer.Write((byte)Value);
                    break;

                case MetaValueType.Enum16:
                    writer.Write((short)Value);
                    break;

                case MetaValueType.Enum32:
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
