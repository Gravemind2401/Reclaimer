﻿using Newtonsoft.Json.Linq;
using Reclaimer.IO;
using Reclaimer.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class EnumValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public ObservableCollection<Tuple<int, string>> Options { get; }

        public EnumValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            Options = new ObservableCollection<Tuple<int, string>>();
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
                    foreach (var n in Node.GetChildElements())
                    {
                        if (n.Name.ToUpper() != "OPTION")
                            continue;

                        var val = n.GetIntAttribute("value");
                        var label = n.GetStringAttribute("name");

                        if (val >= 0)
                            Options.Add(Tuple.Create(val.Value, label));
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

        public override JToken GetJValue() => new JValue(Value);
    }
}
