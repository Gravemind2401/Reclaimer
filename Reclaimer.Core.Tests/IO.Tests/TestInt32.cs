using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestInt32
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Int32Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new Random().Next(int.MinValue, int.MaxValue);

                writer.Write(unchecked((int)0x01000000));
                writer.Write(unchecked((int)0x7F7F7F7F));
                writer.Write(unchecked((int)0xFFFFFFFF));
                writer.Write((int)rand);

                Assert.AreEqual(stream.Length, 16);

                stream.Position = 0;
                Assert.AreEqual(unchecked((int)0x01000000), reader.PeekInt32());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((int)0x01000000), reader.ReadInt32());
                Assert.AreEqual(unchecked((int)0x7F7F7F7F), reader.ReadInt32());
                Assert.AreEqual(unchecked((int)0xFFFFFFFF), reader.ReadInt32());
                Assert.AreEqual(rand, reader.ReadInt32());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void Int32Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = new Random().Next(int.MinValue, int.MaxValue);

                var bytes = BitConverter.GetBytes((int)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((int)0x01000000));
                writer.Write(unchecked((int)0xFFFFFFFF));
                writer.Write(unchecked((int)0xFF00FF00));
                writer.Write((int)rand);

                Assert.AreEqual(stream.Length, 16);

                stream.Position = 0;
                Assert.AreEqual(unchecked((int)0x00000001), reader.PeekInt32());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((int)0x00000001), reader.ReadInt32());
                Assert.AreEqual(unchecked((int)0xFFFFFFFF), reader.ReadInt32());
                Assert.AreEqual(unchecked((int)0x00FF00FF), reader.ReadInt32());
                Assert.AreEqual(BitConverter.ToInt32(bytes, 0), reader.ReadInt32());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
