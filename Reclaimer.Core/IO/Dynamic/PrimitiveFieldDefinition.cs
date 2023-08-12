using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    internal class PrimitiveFieldDefinition<TClass, TField> : FieldDefinition<TClass, TField>
    {
        public PrimitiveFieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, offset, byteOrder)
        { }

        protected override TField StreamRead(EndianReader reader, in ByteOrder? byteOrder)
        {
            return byteOrder.HasValue && DelegateHelper<TField>.InvokeByteOrderRead != null
                ? DelegateHelper<TField>.InvokeByteOrderRead.Invoke(reader, byteOrder.Value)
                : DelegateHelper<TField>.InvokeDefaultRead(reader);
        }

        protected override void StreamWrite(EndianWriter writer, TField value, in ByteOrder? byteOrder)
        {
            if (byteOrder.HasValue && DelegateHelper<TField>.InvokeByteOrderWrite != null)
                DelegateHelper<TField>.InvokeByteOrderWrite(writer, value, byteOrder.Value);
            else
                DelegateHelper<TField>.InvokeDefaultWrite(writer, value);
        }
    }
}
