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

        private StringId stringIdValue;
        public StringId StringIdValue
        {
            get { return stringIdValue; }
            set
            {
                if (SetMetaProperty(ref stringIdValue, value))
                {
                    _value = stringIdValue.Value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                if (SetMetaProperty(ref _value, value))
                {
                    stringIdValue = default(StringId);
                    RaisePropertyChanged(nameof(StringIdValue));
                }
            }
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

                if (FieldDefinition.ValueType == MetaValueType.String)
                    Value = reader.ReadNullTerminatedString(Length);
                else
                {
                    StringIdValue = new StringId(reader, context.Cache);
                    Value = StringIdValue.Value;
                }

                IsDirty = false;
            }
            catch { IsEnabled = false; }

            IsBusy = false;
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
