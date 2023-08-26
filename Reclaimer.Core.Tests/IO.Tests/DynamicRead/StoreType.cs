namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_StoreType01(ByteOrder order)
        {
            StoreType01<StoreTypeClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_StoreType01(ByteOrder order)
        {
            StoreType01<StoreTypeClass01_Builder>(order);
        }

        private static void StoreType01<T>(ByteOrder order)
            where T : StoreTypeClass01
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
                var obj = (T)reader.ReadObject(typeof(T));

                Assert.AreEqual(rand[0], (short)obj.Property1);
                Assert.AreEqual(rand[1], (byte)obj.Property2);
                Assert.AreEqual(rand[2], (float)obj.Property3);
            }
        }
    }
}
