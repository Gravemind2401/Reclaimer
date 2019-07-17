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
    public class StringValue : MetaValue
    {
        internal override string EntryString => Value;

        public int Length { get; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public StringValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            Length = GetIntAttribute(node, "length", "maxlength", "size") ?? 0;
            RefreshValue(reader);
        }

        public override void RefreshValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (ValueType == MetaValueType.String)
                    Value = reader.ReadNullTerminatedString();
                else
                {
                    var id = cache.CacheType < CacheType.Halo3Beta ? reader.ReadInt16() : reader.ReadInt32();
                    Value = cache.StringIndex[id];
                }
            }
            catch { IsEnabled = false; }
        }
    }
}
