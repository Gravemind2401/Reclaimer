namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_DataLength01(ByteOrder order)
        {
            DataLength01<DataLengthClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_DataLength01(ByteOrder order)
        {
            DataLength01<DataLengthClass01_Builder>(order);
        }

        private static void DataLength01<T>(ByteOrder order)
            where T : DataLengthClass01, new()
        {
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var obj = new T
                {
                    Property1 = 5,
                    Property2 = 100
                };

                writer.WriteObject(obj);

                Assert.AreEqual(100, stream.Position);
                stream.Position = 0;
                Assert.AreEqual(5, reader.ReadInt32());
                Assert.AreEqual(100, reader.ReadInt32());

                stream.Position = 0;
                obj = new T
                {
                    Property1 = 7,
                    Property2 = 45
                };

                writer.WriteObject(obj);

                Assert.AreEqual(45, stream.Position);
                stream.Position = 0;
                Assert.AreEqual(7, reader.ReadInt32());
                Assert.AreEqual(45, reader.ReadInt32());
            }
        }
    }
}
