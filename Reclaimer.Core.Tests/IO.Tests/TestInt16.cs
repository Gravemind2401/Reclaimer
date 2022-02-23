using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests
{
    [TestClass]
    public class TestInt16
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Int16Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new Random().Next(short.MinValue, short.MaxValue);

                writer.Write(unchecked((short)0x0100));
                writer.Write(unchecked((short)0x7F7F));
                writer.Write(unchecked((short)0xFFFF));
                writer.Write((short)rand);

                Assert.AreEqual(stream.Length, 8);

                stream.Position = 0;
                Assert.AreEqual(unchecked((short)0x0100), reader.PeekInt16());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((short)0x0100), reader.ReadInt16());
                Assert.AreEqual(unchecked((short)0x7F7F), reader.ReadInt16());
                Assert.AreEqual(unchecked((short)0xFFFF), reader.ReadInt16());
                Assert.AreEqual(rand, reader.ReadInt16());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void Int16Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = new Random().Next(short.MinValue, short.MaxValue);

                var bytes = BitConverter.GetBytes((short)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((short)0x0100));
                writer.Write(unchecked((short)0xFFFF));
                writer.Write(unchecked((short)0xFF00));
                writer.Write((short)rand);

                Assert.AreEqual(stream.Length, 8);

                stream.Position = 0;
                Assert.AreEqual(unchecked((short)0x0001), reader.PeekInt16());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((short)0x0001), reader.ReadInt16());
                Assert.AreEqual(unchecked((short)0xFFFF), reader.ReadInt16());
                Assert.AreEqual(unchecked((short)0x00FF), reader.ReadInt16());
                Assert.AreEqual(BitConverter.ToInt16(bytes, 0), reader.ReadInt16());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
