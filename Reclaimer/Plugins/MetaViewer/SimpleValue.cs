using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
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

        public SimpleValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
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
                    case MetaValueType.Int8: Value = reader.ReadByte(); break;
                    case MetaValueType.Int16: Value = reader.ReadInt16(); break;
                    case MetaValueType.Int32: Value = reader.ReadInt32(); break;
                    case MetaValueType.Int64: Value = reader.ReadInt64(); break;
                    case MetaValueType.UInt16: Value = reader.ReadUInt16(); break;
                    case MetaValueType.UInt32: Value = reader.ReadUInt32(); break;
                    case MetaValueType.UInt64: Value = reader.ReadUInt64(); break;
                    case MetaValueType.Float32: Value = reader.ReadSingle(); break;

                    case MetaValueType.Undefined:
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
                case MetaValueType.Int8: writer.Write((byte)Value); break;
                case MetaValueType.Int16: writer.Write((short)Value); break;
                case MetaValueType.Int32: writer.Write((int)Value); break;
                case MetaValueType.Int64: writer.Write((long)Value); break;
                case MetaValueType.UInt16: writer.Write((ushort)Value); break;
                case MetaValueType.UInt32: writer.Write((uint)Value); break;
                case MetaValueType.UInt64: writer.Write((ulong)Value); break;
                case MetaValueType.Float32: writer.Write((float)Value); break;

                case MetaValueType.Undefined:
                default:
                    writer.Write((int)Value);
                    break;
            }

            IsDirty = false;
        }
    }
}
