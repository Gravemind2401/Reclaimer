namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestHalf
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void HalfSame(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = BitConverter.ToHalf(BitConverter.GetBytes(new Random().NextDouble() * ushort.MaxValue).Reverse().ToArray(), 0);

                writer.Write(Half.Epsilon);
                writer.Write(Half.MinValue);
                writer.Write(Half.MaxValue);
                writer.Write(Half.NaN);
                writer.Write(Half.NegativeInfinity);
                writer.Write(Half.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 14);

                stream.Position = 0;
                Assert.AreEqual(Half.Epsilon, reader.PeekHalf());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(Half.Epsilon, reader.ReadHalf());
                Assert.AreEqual(Half.MinValue, reader.ReadHalf());
                Assert.AreEqual(Half.MaxValue, reader.ReadHalf());
                Assert.AreEqual(Half.NaN, reader.ReadHalf());
                Assert.AreEqual(Half.NegativeInfinity, reader.ReadHalf());
                Assert.AreEqual(Half.PositiveInfinity, reader.ReadHalf());
                Assert.AreEqual(rand, reader.ReadHalf());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void HalfMixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = BitConverter.ToHalf(BitConverter.GetBytes(new Random().NextDouble() * ushort.MaxValue).Reverse().ToArray(), 0);

                var bytes = BitConverter.GetBytes((Half)rand);
                Array.Reverse(bytes);

                writer.Write(Half.Epsilon);
                writer.Write(Half.MinValue);
                writer.Write(Half.MaxValue);
                writer.Write(Half.NaN);
                writer.Write(Half.NegativeInfinity);
                writer.Write(Half.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 14);

                stream.Position = 0;
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.Epsilon).Reverse().ToArray(), 0), reader.PeekHalf());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.Epsilon).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.MinValue).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.MaxValue).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.NaN).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.NegativeInfinity).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(Half.PositiveInfinity).Reverse().ToArray(), 0), reader.ReadHalf());
                Assert.AreEqual(BitConverter.ToHalf(BitConverter.GetBytes(rand).Reverse().ToArray(), 0), reader.ReadHalf());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
