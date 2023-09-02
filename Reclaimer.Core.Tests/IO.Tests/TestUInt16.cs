namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestUInt16
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void UInt16Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new Random().Next(ushort.MinValue, ushort.MaxValue);

                writer.Write(unchecked((ushort)0x0100));
                writer.Write(unchecked((ushort)0x7F7F));
                writer.Write(unchecked((ushort)0xFFFF));
                writer.Write((ushort)rand);

                Assert.AreEqual(stream.Length, 8);

                stream.Position = 0;
                Assert.AreEqual(unchecked((ushort)0x0100), reader.PeekUInt16());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((ushort)0x0100), reader.ReadUInt16());
                Assert.AreEqual(unchecked((ushort)0x7F7F), reader.ReadUInt16());
                Assert.AreEqual(unchecked((ushort)0xFFFF), reader.ReadUInt16());
                Assert.AreEqual(rand, reader.ReadUInt16());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void UInt16Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = new Random().Next(ushort.MinValue, ushort.MaxValue);

                var bytes = BitConverter.GetBytes((ushort)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((ushort)0x0100));
                writer.Write(unchecked((ushort)0xFFFF));
                writer.Write(unchecked((ushort)0xFF00));
                writer.Write((ushort)rand);

                Assert.AreEqual(stream.Length, 8);

                stream.Position = 0;
                Assert.AreEqual(unchecked((ushort)0x0001), reader.PeekUInt16());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((ushort)0x0001), reader.ReadUInt16());
                Assert.AreEqual(unchecked((ushort)0xFFFF), reader.ReadUInt16());
                Assert.AreEqual(unchecked((ushort)0x00FF), reader.ReadUInt16());
                Assert.AreEqual(BitConverter.ToUInt16(bytes, 0), reader.ReadUInt16());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
