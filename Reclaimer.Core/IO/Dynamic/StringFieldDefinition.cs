namespace Reclaimer.IO.Dynamic
{
    internal class StringFieldDefinition<TClass> : FieldDefinition<TClass, string>
    {
        private bool isFixedLength;
        private bool isNullTerminated;
        private bool isLengthPrefixed;
        private bool trimEnabled;
        private char paddingChar;
        private int length;

        protected override string StreamRead(EndianReader reader, ByteOrder byteOrder)
        {
            if (isFixedLength)
                return reader.ReadString(length, trimEnabled);
            else if (isLengthPrefixed)
                return reader.ReadString(byteOrder);
            else
            {
                return length > 0
                    ? reader.ReadNullTerminatedString(length)
                    : reader.ReadNullTerminatedString();
            }
        }

        protected override void StreamWrite(EndianWriter writer, string value, ByteOrder byteOrder)
        {
            if (isFixedLength)
                writer.WriteStringFixedLength(value, length, paddingChar);
            else if (isLengthPrefixed)
                writer.Write(value, byteOrder);
            else
            {
                if (length > 0 && value?.Length > length)
                    value = value[..length];
                writer.WriteStringNullTerminated(value);
            }
        }
    }
}
