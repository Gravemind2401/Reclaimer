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
    public class MultiValue<T> : MetaValue where T : struct
    {
        private T value1;
        public T Value1
        {
            get { return value1; }
            set { SetMetaProperty(ref value1, value); }
        }

        private T value2;
        public T Value2
        {
            get { return value2; }
            set { SetMetaProperty(ref value2, value); }
        }

        private T value3;
        public T Value3
        {
            get { return value3; }
            set { SetMetaProperty(ref value3, value); }
        }

        private T value4;
        public T Value4
        {
            get { return value4; }
            set { SetMetaProperty(ref value4, value); }
        }

        public string[] Labels { get; }

        public MultiValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            if (FieldDefinition.Axes == AxesDefinition.Point)
                Labels = new[] { "x", "y", "z", "w" };
            else if (FieldDefinition.Axes == AxesDefinition.Vector)
                Labels = new[] { "i", "j", "k", "w" };
            else if (FieldDefinition.Axes == AxesDefinition.Bounds)
                Labels = new[] { "min", "max", string.Empty, string.Empty };

            ReadValue(reader);
        }

        public override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                Value1 = reader.ReadObject<T>();
                Value2 = reader.ReadObject<T>();

                if (FieldDefinition.Components > 2)
                    Value3 = reader.ReadObject<T>();

                if (FieldDefinition.Components > 3)
                    Value4 = reader.ReadObject<T>();

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            writer.WriteObject(Value1);
            writer.WriteObject(Value2);

            if (FieldDefinition.Components > 2)
                writer.WriteObject(Value3);

            if (FieldDefinition.Components > 3)
                writer.WriteObject(Value4);

            IsDirty = false;
        }
    }
}
