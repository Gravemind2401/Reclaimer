using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Reclaimer.Blam.Halo5;
using Reclaimer.IO;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class MultiValue<T> : MetaValue where T : struct
    {
        private T value1;
        public T Value1
        {
            get => value1;
            set => SetMetaProperty(ref value1, value);
        }

        private T value2;
        public T Value2
        {
            get => value2;
            set => SetMetaProperty(ref value2, value);
        }

        private T value3;
        public T Value3
        {
            get => value3;
            set => SetMetaProperty(ref value3, value);
        }

        private T value4;
        public T Value4
        {
            get => value4;
            set => SetMetaProperty(ref value4, value);
        }

        public string[] Labels { get; }

        public MultiValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            switch (FieldDefinition.Axes)
            {
                case AxesDefinition.Point:
                    Labels = new[] { "x", "y", "z", "w" };
                    break;
                case AxesDefinition.Vector:
                case AxesDefinition.Angle:
                    Labels = new[] { "i", "j", "k", "w" };
                    break;
                case AxesDefinition.Bounds:
                    Labels = new[] { "min", "max", string.Empty, string.Empty };
                    break;
                case AxesDefinition.Color:
                    Labels = new[] { "r", "g", "b", "a" };
                    break;
                case AxesDefinition.Plane:
                    Labels = new[] { "i", "j", "k", "d" };
                    break;
                default:
                    Labels = new[] { "a", "b", "c", "d" };
                    break;
            }

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
