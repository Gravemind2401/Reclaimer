using Newtonsoft.Json.Linq;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class SimpleValue : MetaValue
    {
        public override string EntryString => Value.ToString();

        private object _value;
        public object Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public SimpleValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                Value = FieldDefinition.ValueType switch
                {
                    MetaValueType.SByte => reader.ReadSByte(),
                    MetaValueType.Int16 => reader.ReadInt16(),
                    MetaValueType.Int32 => reader.ReadInt32(),
                    MetaValueType.Int64 => reader.ReadInt64(),
                    MetaValueType.Byte => reader.ReadByte(),
                    MetaValueType.UInt16 => reader.ReadUInt16(),
                    MetaValueType.UInt32 => reader.ReadUInt32(),
                    MetaValueType.UInt64 => reader.ReadUInt64(),
                    MetaValueType.Angle or MetaValueType.Float32 => reader.ReadSingle(),
                    _ => (object)reader.ReadInt32()
                };
                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            var parsed = float.Parse(Value.ToString());

            switch (FieldDefinition.ValueType)
            {
                case MetaValueType.SByte: writer.Write((sbyte)parsed); break;
                case MetaValueType.Int16: writer.Write((short)parsed); break;
                case MetaValueType.Int32: writer.Write((int)parsed); break;
                case MetaValueType.Int64: writer.Write((long)parsed); break;
                case MetaValueType.Byte: writer.Write((byte)parsed); break;
                case MetaValueType.UInt16: writer.Write((ushort)parsed); break;
                case MetaValueType.UInt32: writer.Write((uint)parsed); break;
                case MetaValueType.UInt64: writer.Write((ulong)parsed); break;

                case MetaValueType.Angle:
                case MetaValueType.Float32:
                    writer.Write((float)parsed);
                    break;

                case MetaValueType.Undefined:
                default:
                    writer.Write((int)parsed);
                    break;
            }

            IsDirty = false;
        }

        public override JToken GetJValue() => new JValue(Value);

        public void SetValue(object value)
        {
            _value = value;
            RaisePropertyChanged(nameof(Value));
        }
    }
}
