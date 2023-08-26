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

    [StructureDefinition<StringsClass01_Builder, DefinitionBuilder>]
    public class StringsClass01_Builder : StringsClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<StringsClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00).IsLengthPrefixed();
                v.Property(x => x.Property2).HasOffset(0x20).IsFixedLength(32, trim: true);
                v.Property(x => x.Property3).HasOffset(0x40).IsFixedLength(32, padding: '*');
                v.Property(x => x.Property4).HasOffset(0x60).IsNullTerminated();
                v.Property(x => x.Property5).HasOffset(0x80).IsNullTerminated(64);
                v.Property(x => x.Property6).HasOffset(0xC0).IsLengthPrefixed().HasByteOrder(ByteOrder.LittleEndian);
                v.Property(x => x.Property7).HasOffset(0xE0).IsLengthPrefixed().HasByteOrder(ByteOrder.BigEndian);
            }
        }
    }

    [StructureDefinition<StringsClass02_Builder, DefinitionBuilder>]
    public class StringsClass02_Builder : StringsClass02
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<StringsClass02_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion().HasByteOrder(ByteOrder.BigEndian);

                v.Property(x => x.Property1).HasOffset(0x00).IsLengthPrefixed();
                v.Property(x => x.Property2).HasOffset(0x20).IsLengthPrefixed().HasByteOrder(ByteOrder.LittleEndian);
                v.Property(x => x.Property3).HasOffset(0x40).IsLengthPrefixed().HasByteOrder(ByteOrder.BigEndian);
            }
        }
    }
}
