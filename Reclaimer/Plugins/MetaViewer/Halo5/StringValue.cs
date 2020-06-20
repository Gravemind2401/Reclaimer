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

        public StringValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, reader, baseAddress, offset)
        {
            if (FieldDefinition.ValueType == MetaValueType.StringId)
                Length = -1;
            else Length = FieldDefinition.Size;

            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (FieldDefinition.ValueType == MetaValueType.StringId)
                {
                    var hash = new StringHash(reader, header);
                    Value = hash.Value;
                }
                else Value = reader.ReadNullTerminatedString(Length);

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            if (FieldDefinition.ValueType == MetaValueType.StringId)
                throw new NotImplementedException();
            else
                writer.WriteStringFixedLength(Value, Length);

            IsDirty = false;
        }
    }
}
