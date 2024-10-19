using Newtonsoft.Json.Linq;
using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.HaloInfinite;
using Reclaimer.IO;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.HaloInfinite
{
    public class StringValue : MetaValue
    {
        public override string EntryString => Value;

        public int Length { get; }

        private string _value;
        public string Value
        {
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public StringValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            Length = FieldDefinition.ValueType == MetaValueType.StringId ? -1 : FieldDefinition.Size;
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
                else
                    Value = reader.ReadNullTerminatedString(Length);

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

        public override JToken GetJValue() => new JValue(Value);
    }
}
