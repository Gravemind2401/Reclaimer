namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Strings01(ByteOrder order)
        {
            Strings01<StringsClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Strings01(ByteOrder order)
        {
            Strings01<StringsClass01_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Strings02(ByteOrder order)
        {
            Strings02<StringsClass02>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Strings02(ByteOrder order)
        {
            Strings02<StringsClass02_Builder>(order);
        }

        private static void Strings01<T>(ByteOrder order)
            where T : StringsClass01
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
                var obj = reader.ReadObject<T>();

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

        private static void Strings02<T>(ByteOrder order)
            where T : StringsClass02
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
                var obj = reader.ReadObject<T>();

                Assert.AreEqual(value1, obj.Property1);
                Assert.AreEqual(value2, obj.Property2);
                Assert.AreEqual(value3, obj.Property3);
            }
        }
    }
}
