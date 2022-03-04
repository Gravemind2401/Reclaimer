using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestSingle
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void SingleSame(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = BitConverter.ToSingle(BitConverter.GetBytes(new Random().NextDouble() * uint.MaxValue).Reverse().ToArray(), 0);

                writer.Write(float.Epsilon);
                writer.Write(float.MinValue);
                writer.Write(float.MaxValue);
                writer.Write(float.NaN);
                writer.Write(float.NegativeInfinity);
                writer.Write(float.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 28);

                stream.Position = 0;
                Assert.AreEqual(float.Epsilon, reader.PeekSingle());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(float.Epsilon, reader.ReadSingle());
                Assert.AreEqual(float.MinValue, reader.ReadSingle());
                Assert.AreEqual(float.MaxValue, reader.ReadSingle());
                Assert.AreEqual(float.NaN, reader.ReadSingle());
                Assert.AreEqual(float.NegativeInfinity, reader.ReadSingle());
                Assert.AreEqual(float.PositiveInfinity, reader.ReadSingle());
                Assert.AreEqual(rand, reader.ReadSingle());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void SingleMixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = BitConverter.ToSingle(BitConverter.GetBytes(new Random().NextDouble() * uint.MaxValue).Reverse().ToArray(), 0);

                var bytes = BitConverter.GetBytes((float)rand);
                Array.Reverse(bytes);

                writer.Write(float.Epsilon);
                writer.Write(float.MinValue);
                writer.Write(float.MaxValue);
                writer.Write(float.NaN);
                writer.Write(float.NegativeInfinity);
                writer.Write(float.PositiveInfinity);
                writer.Write(rand);

                Assert.AreEqual(stream.Length, 28);

                stream.Position = 0;
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.Epsilon).Reverse().ToArray(), 0), reader.PeekSingle());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.Epsilon).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.MinValue).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.MaxValue).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.NaN).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.NegativeInfinity).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(float.PositiveInfinity).Reverse().ToArray(), 0), reader.ReadSingle());
                Assert.AreEqual(BitConverter.ToSingle(BitConverter.GetBytes(rand).Reverse().ToArray(), 0), reader.ReadSingle());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
