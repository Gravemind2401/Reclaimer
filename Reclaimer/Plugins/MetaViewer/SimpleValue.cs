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
        internal override string EntryString => Value.ToString();

        private object _value;
        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public SimpleValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            RefreshValue(reader);
        }

        public override void RefreshValue(EndianReader reader)
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

                    case MetaValueType.Comment: Value = node.InnerText; break;

                    case MetaValueType.Undefined:
                    default:
                        Value = reader.ReadInt32();
                        break;
                }
            }
            catch { IsEnabled = false; }
        }
    }
}
