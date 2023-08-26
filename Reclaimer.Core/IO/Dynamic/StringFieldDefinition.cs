using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    /// <summary>
    /// Defines a field of type <see cref="string"/>.
    /// </summary>
    /// <inheritdoc cref="FieldDefinition{TClass, TField}"/>
    internal class StringFieldDefinition<TClass> : FieldDefinition<TClass, string>
    {
        private readonly bool isInterned;
        private readonly bool isLengthPrefixed;
        private readonly bool isNullTerminated;
        private readonly bool isFixedLength;
        private readonly bool trimEnabled;
        private readonly char paddingChar;
        private readonly int length;

        private StringFieldDefinition(
            PropertyInfo targetProperty,
            long offset,
            ByteOrder? byteOrder,
            bool isInterned = default,
            bool isLengthPrefixed = default,
            bool isFixedLength = default,
            bool isNullTerminated = default,
            int length = default,
            char paddingChar = default,
            bool trimEnabled = default)
            : base(targetProperty, offset, byteOrder)
        {
            this.isInterned = isInterned;
            this.isLengthPrefixed = isLengthPrefixed;
            this.isFixedLength = isFixedLength;
            this.isNullTerminated = isNullTerminated;
            this.length = length;
            this.paddingChar = paddingChar;
            this.trimEnabled = trimEnabled;
        }

        public static StringFieldDefinition<TClass> FromAttributes(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            var isInterned = Attribute.IsDefined(targetProperty, typeof(InternedAttribute));

            if (Attribute.IsDefined(targetProperty, typeof(LengthPrefixedAttribute)))
                return new StringFieldDefinition<TClass>(targetProperty, offset, byteOrder, isInterned, isLengthPrefixed: true);

            var fixedLength = targetProperty.GetCustomAttribute<FixedLengthAttribute>();
            if (fixedLength != null)
                return new StringFieldDefinition<TClass>(targetProperty, offset, byteOrder, isInterned, isFixedLength: true, length: fixedLength.Length, paddingChar: fixedLength.Padding, trimEnabled: fixedLength.Trim);

            var nullTerminated = targetProperty.GetCustomAttribute<NullTerminatedAttribute>();
            if (nullTerminated != null)
                return new StringFieldDefinition<TClass>(targetProperty, offset, byteOrder, isInterned, isNullTerminated: true, length: nullTerminated.Length);

            throw Exceptions.StringTypeUnknown(targetProperty);
        }

        public static StringFieldDefinition<TClass> LengthPrefixed(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder, bool isInterned)
        {
            return new StringFieldDefinition<TClass>(targetProperty, offset, byteOrder, isInterned, isLengthPrefixed: true);
        }

        public static StringFieldDefinition<TClass> FixedLength(PropertyInfo targetProperty, long offset, bool isInterned, int length, char paddingChar, bool trimEnabled)
        {
            return new StringFieldDefinition<TClass>(targetProperty, offset, default, isInterned, isFixedLength: true, length: length, paddingChar: paddingChar, trimEnabled: trimEnabled);
        }

        public static StringFieldDefinition<TClass> NullTerminated(PropertyInfo targetProperty, long offset, bool isInterned, int length)
        {
            return new StringFieldDefinition<TClass>(targetProperty, offset, default, isInterned, isNullTerminated: true, length: length);
        }

        protected override string StreamRead(EndianReader reader, in ByteOrder? byteOrder)
        {
            string value;

            if (isFixedLength)
                value = reader.ReadString(length, trimEnabled);
            else if (isLengthPrefixed)
                value = byteOrder.HasValue ? reader.ReadString(byteOrder.Value) : reader.ReadString();
            else
            {
                value = length > 0
                    ? reader.ReadNullTerminatedString(length)
                    : reader.ReadNullTerminatedString();
            }

            if (isInterned)
                value = string.Intern(value);

            return value;
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
