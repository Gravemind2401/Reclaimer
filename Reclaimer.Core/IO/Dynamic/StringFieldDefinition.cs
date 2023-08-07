using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    internal class StringFieldDefinition<TClass> : FieldDefinition<TClass, string>
    {
        private readonly bool isFixedLength;
        private readonly bool isNullTerminated;
        private readonly bool isLengthPrefixed;
        private readonly bool trimEnabled;
        private readonly char paddingChar;
        private readonly int length;

        public StringFieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, offset, byteOrder)
        {
            if (Attribute.IsDefined(targetProperty, typeof(LengthPrefixedAttribute)))
            {
                isLengthPrefixed = true;
                return;
            }

            var fixedLength = targetProperty.GetCustomAttribute<FixedLengthAttribute>();
            if (fixedLength != null)
            {
                isFixedLength = true;
                length = fixedLength.Length;
                paddingChar = fixedLength.Padding;
                trimEnabled = fixedLength.Trim;
                return;
            }

            var nullTerminated = targetProperty.GetCustomAttribute<NullTerminatedAttribute>();
            if (nullTerminated != null)
            {
                isNullTerminated = true;
                length = nullTerminated.Length;
                return;
            }
        }

        protected override string StreamRead(EndianReader reader, in ByteOrder? byteOrder)
        {
            if (isFixedLength)
                return reader.ReadString(length, trimEnabled);
            else if (isLengthPrefixed)
                return byteOrder.HasValue ? reader.ReadString(byteOrder.Value) : reader.ReadString();
            else
            {
                return length > 0
                    ? reader.ReadNullTerminatedString(length)
                    : reader.ReadNullTerminatedString();
            }
        }

        protected override void StreamWrite(EndianWriter writer, string value, in ByteOrder? byteOrder)
        {
            if (isFixedLength)
                writer.WriteStringFixedLength(value, length, paddingChar);
            else if (isLengthPrefixed)
            {
                if (byteOrder.HasValue)
                    writer.Write(value, byteOrder.Value);
                else
                    writer.Write(value);
            }
            else
            {
                if (length > 0 && value?.Length > length)
                    value = value[..length];
                writer.WriteStringNullTerminated(value);
            }
        }
    }
}
