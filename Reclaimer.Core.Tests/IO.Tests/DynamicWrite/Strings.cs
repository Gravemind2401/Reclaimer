namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Strings01(ByteOrder order)
        {
            var obj = new StringsClass01();

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
        public void Strings02(ByteOrder order)
        {
            var obj = new StringsClass02
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
    }
}
