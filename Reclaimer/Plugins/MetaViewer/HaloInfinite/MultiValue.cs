using Newtonsoft.Json.Linq;
using Reclaimer.Blam.HaloInfinite;
using Reclaimer.IO;
using System.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.HaloInfinite
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
            Labels = FieldDefinition.Axes switch
            {
                AxesDefinition.Point => new[] { "x", "y", "z", "w" },
                AxesDefinition.Vector or AxesDefinition.Angle => new[] { "i", "j", "k", "w" },
                AxesDefinition.Bounds => new[] { "min", "max", string.Empty, string.Empty },
                AxesDefinition.Color => new[] { "r", "g", "b", "a" },
                AxesDefinition.Plane => new[] { "i", "j", "k", "d" },
                _ => new[] { "a", "b", "c", "d" }
            };
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

        public override JToken GetJValue()
        {
            var result = new JObject();

            for (var i = 0; i < FieldDefinition.Components; i++)
            {
                if (i < Labels.Length && !string.IsNullOrEmpty(Labels[i]))
                {
                    var value = i switch
                    {
                        0 => Value1,
                        1 => Value2,
                        2 => Value3,
                        3 => Value4,
                        _ => default
                    };

                    // Only add if the value is valid
                    if (i < 2 || FieldDefinition.Components > i)
                    {
                        result.Add(Labels[i], new JValue(value));
                    }
                }
            }

            return result;
        }
    }
}
