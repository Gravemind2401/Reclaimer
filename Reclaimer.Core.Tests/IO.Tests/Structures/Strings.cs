namespace Reclaimer.IO.Tests.Structures
{
    public class StringsClass01
    {
        [Offset(0x00)]
        [LengthPrefixed]
        public string Property1 { get; set; }

        [Offset(0x20)]
        [FixedLength(32, Trim = true)]
        public string Property2 { get; set; }

        [Offset(0x40)]
        [FixedLength(32, Padding = '*')]
        public string Property3 { get; set; }

        [Offset(0x60)]
        [NullTerminated]
        public string Property4 { get; set; }

        [Offset(0x80)]
        [NullTerminated(Length = 64)]
        public string Property5 { get; set; }

        [Offset(0xC0)]
        [LengthPrefixed]
        [ByteOrder(ByteOrder.LittleEndian)]
        public string Property6 { get; set; }

        [Offset(0xE0)]
        [LengthPrefixed]
        [ByteOrder(ByteOrder.BigEndian)]
        public string Property7 { get; set; }
    }

    [ByteOrder(ByteOrder.BigEndian)]
    public class StringsClass02
    {
        [Offset(0x00)]
        [LengthPrefixed]
        public string Property1 { get; set; }

        [Offset(0x20)]
        [LengthPrefixed]
        [ByteOrder(ByteOrder.LittleEndian)]
        public string Property2 { get; set; }

        [Offset(0x40)]
        [LengthPrefixed]
        [ByteOrder(ByteOrder.BigEndian)]
        public string Property3 { get; set; }
    }
}
