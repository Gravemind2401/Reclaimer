namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Enums01(ByteOrder order)
        {
            Enums01<EnumClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Enums01(ByteOrder order)
        {
            Enums01<EnumClass01_Builder>(order);
        }

        private static void Enums01<T>(ByteOrder order)
            where T : EnumClass01, new()
        {
            var rng = new Random();
            var obj = new T
            {
                Property1 = (Enum8)rng.Next(1, 4),
                Property2 = (Enum16)rng.Next(4, 7),
                Property3 = (Enum32)rng.Next(7, 10),
                Property4 = (Enum64)rng.Next(10, 13),
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual((byte)obj.Property1, reader.ReadByte());
                Assert.AreEqual((short)obj.Property2, reader.ReadInt16());
                Assert.AreEqual((int)obj.Property3, reader.ReadInt32());
                Assert.AreEqual((long)obj.Property4, reader.ReadInt64());
            }
        }
    }
}
