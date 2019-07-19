using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public class EnumValue : MetaValue
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public ObservableCollection<Tuple<int, string>> Options { get; }

        public EnumValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            Options = new ObservableCollection<Tuple<int, string>>();

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
                    case MetaValueType.Enum8:
                        Value = reader.ReadByte();
                        break;

                    case MetaValueType.Enum16:
                        Value = reader.ReadInt16();
                        break;

                    case MetaValueType.Enum32:
                        Value = reader.ReadInt32();
                        break;

                    default:
                        IsEnabled = false;
                        break;
                }

                Options.Clear();
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.Name.ToUpper() != "OPTION")
                        continue;

                    var val = GetIntAttribute(n, "value");
                    var label = GetStringAttribute(n, "name");

                    if (val >= 0)
                        Options.Add(Tuple.Create(val.Value, label));
                }
            }
            catch { IsEnabled = false; }
        }
    }
}
