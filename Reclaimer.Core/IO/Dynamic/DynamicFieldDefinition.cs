using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    internal class DynamicFieldDefinition<TClass, TField> : FieldDefinition<TClass, TField>
    {
        public DynamicFieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, offset, byteOrder)
        { }

        protected override TField StreamRead(EndianReader reader, in ByteOrder? byteOrder) => reader.ReadObject<TField>();
        protected override void StreamWrite(EndianWriter writer, TField value, in ByteOrder? byteOrder) => writer.WriteObject(value);
    }
}
