namespace Reclaimer.IO.Dynamic
{
    internal class PrimitiveFieldDefinition<TClass, TField> : FieldDefinition<TClass, TField>
        where TField : struct, IComparable, IComparable<TField>, IEquatable<TField>
    {
        protected override TField StreamRead(EndianReader reader, ByteOrder byteOrder)
        {
            return DelegateHelper<TField>.InvokeByteOrderRead?.Invoke(reader, byteOrder)
                ?? DelegateHelper<TField>.InvokeDefaultRead(reader);
        }

        protected override void StreamWrite(EndianWriter writer, TField value, ByteOrder byteOrder)
        {
            if (DelegateHelper<TField>.InvokeByteOrderWrite != null)
                DelegateHelper<TField>.InvokeByteOrderWrite(writer, value, byteOrder);
            else
                DelegateHelper<TField>.InvokeDefaultWrite(writer, value);
        }
    }
}
