using Newtonsoft.Json.Linq;
using Reclaimer.IO;
using System.IO;
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
            get => _value;
            set => SetMetaProperty(ref _value, value);
        }

        public StringValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
        {
            Length = node.GetIntAttribute("length", "maxlength", "size") ?? 0;
            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsBusy = true;
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);
                Value = reader.ReadNullTerminatedString(Length);

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);
            writer.WriteStringFixedLength(Value, Length, '\0');

            IsDirty = false;
        }

        public override JToken GetJValue() => new JValue(Value);
    }
}
