namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void DataLength01(ByteOrder order)
        {
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var obj = new DataLengthClass01
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
                obj = new DataLengthClass01
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
