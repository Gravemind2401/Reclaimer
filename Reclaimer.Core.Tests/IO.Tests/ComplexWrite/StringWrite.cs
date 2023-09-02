namespace Reclaimer.IO.Tests.ComplexWrite
{
    public partial class ComplexWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void WriteStrings01(ByteOrder order)
        {
            var obj = new DataClass05();

            obj.Property1 = obj.Property6 = obj.Property7 = "Length_Prefixed_String_#01!";
            obj.Property2 = obj.Property3 = "Fixed_Length_String_#01!";
            obj.Property4 = obj.Property5 = "Null_Terminated_String_#01!";

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadString());

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadString(32, true));

                reader.Seek(0x40, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadString(32, false).TrimEnd('*'));

                reader.Seek(0x60, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property4, reader.ReadNullTerminatedString());

                reader.Seek(0x80, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property5, reader.ReadString(64, false).TrimEnd('\0'));

                reader.Seek(0xC0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property6, reader.ReadString(ByteOrder.LittleEndian));

                reader.Seek(0xE0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property7, reader.ReadString(ByteOrder.BigEndian));
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void WriteStrings02(ByteOrder order)
        {
            var obj = new DataClass06
            {
                Property1 = "Length_Prefixed_String_#01!",
                Property2 = "Length_Prefixed_String_#02!",
                Property3 = "Length_Prefixed_String_#03!"
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadString(ByteOrder.BigEndian));

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadString(ByteOrder.LittleEndian));

                reader.Seek(0x40, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadString(ByteOrder.BigEndian));
            }
        }

        public class DataClass05
        {
            [Offset(0x00)]
            [LengthPrefixed]
            public string Property1 { get; set; }

            [Offset(0x20)]
            [FixedLength(32)]
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
        public class DataClass06
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
}
