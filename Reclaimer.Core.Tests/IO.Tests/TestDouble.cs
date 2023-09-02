namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestDouble
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void DoubleSame(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = BitConverter.ToDouble(BitConverter.GetBytes(new Random().NextDouble() * ulong.MaxValue).Reverse().ToArray(), 0);

                writer.Write(double.Epsilon);
                writer.Write(double.MinValue);
                writer.Write(double.MaxValue);
                writer.Write(double.NaN);
                writer.Write(double.NegativeInfinity);
                writer.Write(double.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 56);

                stream.Position = 0;
                Assert.AreEqual(double.Epsilon, reader.PeekDouble());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(double.Epsilon, reader.ReadDouble());
                Assert.AreEqual(double.MinValue, reader.ReadDouble());
                Assert.AreEqual(double.MaxValue, reader.ReadDouble());
                Assert.AreEqual(double.NaN, reader.ReadDouble());
                Assert.AreEqual(double.NegativeInfinity, reader.ReadDouble());
                Assert.AreEqual(double.PositiveInfinity, reader.ReadDouble());
                Assert.AreEqual(rand, reader.ReadDouble());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void DoubleMixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = BitConverter.ToDouble(BitConverter.GetBytes(new Random().NextDouble() * ulong.MaxValue).Reverse().ToArray(), 0);

                var bytes = BitConverter.GetBytes(rand);
                Array.Reverse(bytes);

                writer.Write(double.Epsilon);
                writer.Write(double.MinValue);
                writer.Write(double.MaxValue);
                writer.Write(double.NaN);
                writer.Write(double.NegativeInfinity);
                writer.Write(double.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 56);

                stream.Position = 0;
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.Epsilon).Reverse().ToArray(), 0), reader.PeekDouble());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.Epsilon).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.MinValue).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.MaxValue).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.NaN).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.NegativeInfinity).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(double.PositiveInfinity).Reverse().ToArray(), 0), reader.ReadDouble());
                Assert.AreEqual(BitConverter.ToDouble(BitConverter.GetBytes(rand).Reverse().ToArray(), 0), reader.ReadDouble());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
