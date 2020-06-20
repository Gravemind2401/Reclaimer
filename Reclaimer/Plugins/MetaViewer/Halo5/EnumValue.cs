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
    public class EnumValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetMetaProperty(ref _value, value); }
        }

        public ObservableCollection<Tuple<int, string>> Options { get; }

        public EnumValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, reader, baseAddress, offset)
        {
            Options = new ObservableCollection<Tuple<int, string>>();
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
                    case MetaValueType.Enum8:
                        Value = reader.ReadByte();
                        break;

                    case MetaValueType.Enum16:
                        Value = reader.ReadInt16();
                        break;

                    case MetaValueType.Enum32:
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

                        var label = n.GetStringAttribute("name");
                        Options.Add(Tuple.Create(index++, label));
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
    }
}
