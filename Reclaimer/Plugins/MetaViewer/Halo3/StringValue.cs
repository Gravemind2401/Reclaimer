using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class StringValue : MetaValue
    {
        public override string EntryString => Value;

        public int Length { get; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetMetaProperty(ref _value, value); }
        }

        public StringValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            Length = node.GetIntAttribute("length", "maxlength", "size") ?? 0;
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (FieldDefinition.ValueType == MetaValueType.String)
                    Value = reader.ReadNullTerminatedString(Length);
                else
                {
                    var id = context.Cache.CacheType < CacheType.Halo3Beta ? reader.ReadInt16() : reader.ReadInt32();
                    Value = context.Cache.StringIndex[id];
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            if (FieldDefinition.ValueType == MetaValueType.String)
                writer.WriteStringFixedLength(Value, Length);
            else
                throw new NotImplementedException();

            IsDirty = false;
        }
    }
}
