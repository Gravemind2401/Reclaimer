namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Strings01(ByteOrder order)
        {
            var value1 = "Length_Prefixed_String_#01!";
            var value2 = "Fixed_Length_String_#01!";
            var value3 = "Null_Terminated_String_#01!";

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Write(value1);

                stream.Position = 0x20;
                writer.WriteStringFixedLength(value2, 32);

                stream.Position = 0x40;
                writer.WriteStringFixedLength(value2, 32);

                stream.Position = 0x60;
                writer.WriteStringNullTerminated(value3);

                stream.Position = 0x80;
                writer.WriteStringNullTerminated(value3);

                stream.Position = 0xC0;
                writer.Write(value1, ByteOrder.LittleEndian);

                stream.Position = 0xE0;
                writer.Write(value1, ByteOrder.BigEndian);

                stream.Position = 0;
                var obj = reader.ReadObject<StringsClass01>();

                Assert.AreEqual(value1, obj.Property1);

                Assert.AreEqual(value2, obj.Property2);

                Assert.AreEqual(32, obj.Property3.Length);
                Assert.IsTrue(obj.Property3.StartsWith(value2));

                Assert.AreEqual(value3, obj.Property4);
                Assert.AreEqual(value3, obj.Property5);

                Assert.AreEqual(value1, obj.Property6);
                Assert.AreEqual(value1, obj.Property7);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Strings02(ByteOrder order)
        {
            var value1 = "Length_Prefixed_String_#01!";
            var value2 = "Length_Prefixed_String_#02!";
            var value3 = "Length_Prefixed_String_#03!";

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Write(value1, ByteOrder.BigEndian);

                stream.Position = 0x20;
                writer.Write(value2, ByteOrder.LittleEndian);

                stream.Position = 0x40;
                writer.Write(value3, ByteOrder.BigEndian);

                stream.Position = 0;
                var obj = reader.ReadObject<StringsClass02>();

                Assert.AreEqual(value1, obj.Property1);
                Assert.AreEqual(value2, obj.Property2);
                Assert.AreEqual(value3, obj.Property3);
            }
        }
    }
}
