namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void StoreType01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[3];

                rand[0] = (short)rng.Next(short.MinValue, short.MaxValue);
                writer.Write((short)rand[0]);

                rand[1] = (byte)rng.Next(byte.MinValue, byte.MaxValue);
                writer.Write((byte)rand[1]);

                rand[2] = (float)rng.NextDouble();
                writer.Write((float)rand[2]);

                stream.Position = 0;
                var obj = (StoreTypeClass01)reader.ReadObject(typeof(StoreTypeClass01));

                Assert.AreEqual(rand[0], (short)obj.Property1);
                Assert.AreEqual(rand[1], (byte)obj.Property2);
                Assert.AreEqual(rand[2], (float)obj.Property3);
            }
        }
    }
}
