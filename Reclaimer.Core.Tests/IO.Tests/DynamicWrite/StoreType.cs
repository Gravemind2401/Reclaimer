namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
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
            where T : StoreTypeClass01, new()
        {
            var rng = new Random();
            var obj = new T
            {
                Property1 = rng.Next(short.MinValue, short.MaxValue),
                Property2 = rng.Next(byte.MinValue, byte.MaxValue),
                Property3 = rng.NextDouble()
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual((short)obj.Property1, reader.ReadInt16());
                Assert.AreEqual((byte)obj.Property2, reader.ReadByte());
                Assert.AreEqual((float)obj.Property3, reader.ReadSingle());
            }
        }
    }
}
