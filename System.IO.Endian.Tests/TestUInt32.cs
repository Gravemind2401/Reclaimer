using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests
{
    [TestClass]
    public class TestUInt32
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void UInt32Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = (uint)(new Random().NextDouble() * uint.MaxValue);

                writer.Write(unchecked((uint)0x01000000));
                writer.Write(unchecked((uint)0x7F7F7F7F));
                writer.Write(unchecked((uint)0xFFFFFFFF));
                writer.Write((uint)rand);

                Assert.AreEqual(stream.Length, 16);

                stream.Position = 0;
                Assert.AreEqual(unchecked((uint)0x01000000), reader.PeekUInt32());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((uint)0x01000000), reader.ReadUInt32());
                Assert.AreEqual(unchecked((uint)0x7F7F7F7F), reader.ReadUInt32());
                Assert.AreEqual(unchecked((uint)0xFFFFFFFF), reader.ReadUInt32());
                Assert.AreEqual(rand, reader.ReadUInt32());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void UInt32Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = (uint)(new Random().NextDouble() * uint.MaxValue);

                var bytes = BitConverter.GetBytes((uint)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((uint)0x01000000));
                writer.Write(unchecked((uint)0xFFFFFFFF));
                writer.Write(unchecked((uint)0xFF00FF00));
                writer.Write((uint)rand);

                Assert.AreEqual(stream.Length, 16);

                stream.Position = 0;
                Assert.AreEqual(unchecked((uint)0x00000001), reader.PeekUInt32());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((uint)0x00000001), reader.ReadUInt32());
                Assert.AreEqual(unchecked((uint)0xFFFFFFFF), reader.ReadUInt32());
                Assert.AreEqual(unchecked((uint)0x00FF00FF), reader.ReadUInt32());
                Assert.AreEqual(BitConverter.ToUInt32(bytes, 0), reader.ReadUInt32());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
