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
    public class MultiValue : MetaValue
    {
        private float value1;
        public float Value1
        {
            get { return value1; }
            set { SetMetaProperty(ref value1, value); }
        }

        private float value2;
        public float Value2
        {
            get { return value2; }
            set { SetMetaProperty(ref value2, value); }
        }

        private float value3;
        public float Value3
        {
            get { return value3; }
            set { SetMetaProperty(ref value3, value); }
        }

        private float value4;
        public float Value4
        {
            get { return value4; }
            set { SetMetaProperty(ref value4, value); }
        }

        public string[] Labels { get; }

        public MultiValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
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

                Value1 = reader.ReadSingle();
                Value2 = reader.ReadSingle();

                if (FieldDefinition.Components > 2)
                    Value3 = reader.ReadSingle();

                if (FieldDefinition.Components > 3)
                    Value4 = reader.ReadSingle();

                IsDirty = false;
            }
            catch { IsEnabled = false; }
        }

        public override void WriteValue(EndianWriter writer)
        {
            writer.Seek(ValueAddress, SeekOrigin.Begin);

            writer.Write(Value1);
            writer.Write(Value2);

            if (FieldDefinition.Components > 2)
                writer.Write(Value3);

            if (FieldDefinition.Components > 3)
                writer.Write(Value4);

            IsDirty = false;
        }
    }
}
