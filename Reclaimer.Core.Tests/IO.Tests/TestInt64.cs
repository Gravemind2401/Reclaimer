namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestInt64
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Int64Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = unchecked((long)(ulong)(new Random().NextDouble() * ulong.MaxValue));

                writer.Write(unchecked((long)0x0100000000000000));
                writer.Write(unchecked((long)0x7F7F7F7F7F7F7F7F));
                writer.Write(unchecked((long)0xFFFFFFFFFFFFFFFF));
                writer.Write((long)rand);

                Assert.AreEqual(stream.Length, 32);

                stream.Position = 0;
                Assert.AreEqual(unchecked((long)0x0100000000000000), reader.PeekInt64());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((long)0x0100000000000000), reader.ReadInt64());
                Assert.AreEqual(unchecked((long)0x7F7F7F7F7F7F7F7F), reader.ReadInt64());
                Assert.AreEqual(unchecked((long)0xFFFFFFFFFFFFFFFF), reader.ReadInt64());
                Assert.AreEqual(rand, reader.ReadInt64());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void Int64Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = unchecked((long)(ulong)(new Random().NextDouble() * ulong.MaxValue));

                var bytes = BitConverter.GetBytes((long)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((long)0x0100000000000000));
                writer.Write(unchecked((long)0xFFFFFFFFFFFFFFFF));
                writer.Write(unchecked((long)0xFF00FF00FF00FF00));
                writer.Write((long)rand);

                Assert.AreEqual(stream.Length, 32);

                stream.Position = 0;
                Assert.AreEqual(unchecked((long)0x0000000000000001), reader.PeekInt64());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((long)0x0000000000000001), reader.ReadInt64());
                Assert.AreEqual(unchecked((long)0xFFFFFFFFFFFFFFFF), reader.ReadInt64());
                Assert.AreEqual(unchecked((long)0x00FF00FF00FF00FF), reader.ReadInt64());
                Assert.AreEqual(BitConverter.ToInt64(bytes, 0), reader.ReadInt64());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
