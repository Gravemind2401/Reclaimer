namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Enums01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[4];

                rand[0] = (Enum8)rng.Next(1, 4);
                writer.Write((byte)(Enum8)rand[0]);

                rand[1] = (Enum16)rng.Next(4, 7);
                writer.Write((short)(Enum16)rand[1]);

                rand[2] = (Enum32)rng.Next(7, 10);
                writer.Write((int)(Enum32)rand[2]);

                rand[3] = (Enum64)rng.Next(10, 13);
                writer.Write((long)(Enum64)rand[3]);

                stream.Position = 0;
                var obj = reader.ReadObject<EnumClass01>();

                Assert.AreEqual((Enum8)rand[0], obj.Property1);
                Assert.AreEqual((Enum16)rand[1], obj.Property2);
                Assert.AreEqual((Enum32)rand[2], obj.Property3);
                Assert.AreEqual((Enum64)rand[3], obj.Property4);
            }
        }
    }
}
