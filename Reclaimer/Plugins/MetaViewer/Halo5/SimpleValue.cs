using Adjutant.Blam.Halo5;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class SimpleValue : MetaValue
    {
        public override string EntryString => Value.ToString();

        private object _value;
        public object Value
        {
            get { return _value; }
            set { SetMetaProperty(ref _value, value); }
        }

        public SimpleValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, reader, baseAddress, offset)
        {
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
                    case MetaValueType._field_char_integer:
                    case MetaValueType._field_byte_integer:
                        Value = reader.ReadByte();
                        break;
                    case MetaValueType._field_word_integer:
                    case MetaValueType._field_short_integer:
                        Value = reader.ReadInt16();
                        break;
                    case MetaValueType._field_dword_integer:
                    case MetaValueType._field_long_integer:
                        Value = reader.ReadInt32();
                        break;
                    case MetaValueType._field_int64_integer:
                        Value = reader.ReadInt64();
                        break;
                    case MetaValueType._field_short_block_index:
                        Value = reader.ReadUInt16();
                        break;
                    case MetaValueType._field_long_block_index:
                        Value = reader.ReadUInt32();
                        break;
                    case MetaValueType._field_real:
                    case MetaValueType._field_real_fraction:
                    case MetaValueType._field_angle:
                        Value = reader.ReadSingle();
                        break;

                    default:
                        Value = reader.ReadInt32();
                        break;
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
                case MetaValueType._field_char_integer:
                case MetaValueType._field_byte_integer:
                    writer.Write((byte)Value);
                    break;
                case MetaValueType._field_word_integer:
                case MetaValueType._field_short_integer:
                    writer.Write((short)Value);
                    break;
                case MetaValueType._field_dword_integer:
                case MetaValueType._field_long_integer:
                    writer.Write((int)Value);
                    break;
                case MetaValueType._field_int64_integer:
                case MetaValueType._field_qword_integer:
                    writer.Write((long)Value);
                    break;
                case MetaValueType._field_short_block_index:
                    writer.Write((ushort)Value);
                    break;
                case MetaValueType._field_long_block_index:
                    writer.Write((uint)Value);
                    break;
                case MetaValueType._field_real:
                case MetaValueType._field_real_fraction:
                case MetaValueType._field_angle:
                    writer.Write((float)Value);
                    break;

                default:
                    writer.Write((int)Value);
                    break;
            }

            IsDirty = false;
        }
    }
}
