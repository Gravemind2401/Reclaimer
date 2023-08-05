using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    internal class StringFieldDefinition<TClass> : FieldDefinition<TClass, string>
    {
        public bool IsFixedLength { get; init; }
        public bool IsNullTerminated { get; init; }
        public bool IsLengthPrefixed { get; init; }
        public bool TrimEnabled { get; init; }
        public char PaddingChar { get; init; }
        public int Length { get; init; }

        public override PropertyInfo TargetProperty
        {
            get => base.TargetProperty;
            init
            {
                base.TargetProperty = value;

                if (Attribute.IsDefined(value, typeof(LengthPrefixedAttribute)))
                {
                    IsLengthPrefixed = true;
                    return;
                }

                var fixedLength = value.GetCustomAttribute<FixedLengthAttribute>();
                if (fixedLength != null)
                {
                    IsFixedLength = true;
                    Length = fixedLength.Length;
                    PaddingChar = fixedLength.Padding;
                    TrimEnabled = fixedLength.Trim;
                    return;
                }

                var nullTerminated = value.GetCustomAttribute<NullTerminatedAttribute>();
                if (nullTerminated != null)
                {
                    IsNullTerminated = true;
                    Length = nullTerminated.Length;
                    return;
                }
            }
        }

        protected override string StreamRead(EndianReader reader, ByteOrder byteOrder)
        {
            if (IsFixedLength)
                return reader.ReadString(Length, TrimEnabled);
            else if (IsLengthPrefixed)
                return reader.ReadString(byteOrder);
            else
            {
                return Length > 0
                    ? reader.ReadNullTerminatedString(Length)
                    : reader.ReadNullTerminatedString();
            }
        }

        protected override void StreamWrite(EndianWriter writer, string value, ByteOrder byteOrder)
        {
            if (IsFixedLength)
                writer.WriteStringFixedLength(value, Length, PaddingChar);
            else if (IsLengthPrefixed)
                writer.Write(value, byteOrder);
            else
            {
                if (Length > 0 && value?.Length > Length)
                    value = value[..Length];
                writer.WriteStringNullTerminated(value);
            }
        }
    }
}
