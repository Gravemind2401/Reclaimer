using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests
{
    [TestClass]
    public class TestUInt64
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void UInt64Same(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = (ulong)(new Random().NextDouble() * ulong.MaxValue);

                writer.Write(unchecked((ulong)0x0100000000000000));
                writer.Write(unchecked((ulong)0x7F7F7F7F7F7F7F7F));
                writer.Write(unchecked((ulong)0xFFFFFFFFFFFFFFFF));
                writer.Write((ulong)rand);

                Assert.AreEqual(stream.Length, 32);

                stream.Position = 0;
                Assert.AreEqual(unchecked((ulong)0x0100000000000000), reader.PeekUInt64());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((ulong)0x0100000000000000), reader.ReadUInt64());
                Assert.AreEqual(unchecked((ulong)0x7F7F7F7F7F7F7F7F), reader.ReadUInt64());
                Assert.AreEqual(unchecked((ulong)0xFFFFFFFFFFFFFFFF), reader.ReadUInt64());
                Assert.AreEqual(rand, reader.ReadUInt64());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void UInt64Mixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var rand = (ulong)(new Random().NextDouble() * ulong.MaxValue);

                var bytes = BitConverter.GetBytes((ulong)rand);
                Array.Reverse(bytes);

                writer.Write(unchecked((ulong)0x0100000000000000));
                writer.Write(unchecked((ulong)0xFFFFFFFFFFFFFFFF));
                writer.Write(unchecked((ulong)0xFF00FF00FF00FF00));
                writer.Write((ulong)rand);

                Assert.AreEqual(stream.Length, 32);

                stream.Position = 0;
                Assert.AreEqual(unchecked((ulong)0x0000000000000001), reader.PeekUInt64());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(unchecked((ulong)0x0000000000000001), reader.ReadUInt64());
                Assert.AreEqual(unchecked((ulong)0xFFFFFFFFFFFFFFFF), reader.ReadUInt64());
                Assert.AreEqual(unchecked((ulong)0x00FF00FF00FF00FF), reader.ReadUInt64());
                Assert.AreEqual(BitConverter.ToUInt64(bytes, 0), reader.ReadUInt64());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
