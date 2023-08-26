namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.BigEndian)]
        [DataRow(ByteOrder.LittleEndian)]
        public void StoreType01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new StoreTypeClass01
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
