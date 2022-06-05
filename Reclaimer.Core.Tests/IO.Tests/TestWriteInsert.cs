using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestWriteInsert
    {
        [TestMethod]
        public void TestInsert01()
        {
            using (var stream = new MemoryStream(150))
            using (var reader = new EndianReader(stream))
            using (var writer = new EndianWriter(stream))
            {
                writer.Fill(22, 50);
                Assert.AreEqual(50, reader.BaseStream.Position);
                writer.Fill(44, 50);
                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                var buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));

                writer.Seek(50, SeekOrigin.Begin);
                writer.Insert(99, 50);

                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 99));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));
            }
        }

        [TestMethod]
        public void TestInsert02()
        {
            using (var stream = new MemoryStream(new byte[150]))
            using (var reader = new EndianReader(stream))
            using (var writer = new EndianWriter(stream))
            {
                writer.Fill(22, 50);
                Assert.AreEqual(50, reader.BaseStream.Position);
                writer.Fill(44, 50);
                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                var buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));

                buffer = new byte[50];
                for (byte i = 0; i < buffer.Length; i++)
                    buffer[i] = i;

                writer.Seek(50, SeekOrigin.Begin);
                writer.Insert(buffer);

                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                for (byte i = 0; i < 50; i++)
                    Assert.AreEqual(i, reader.ReadByte());

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));
            }
        }

        [TestMethod]
        public void TestCopy01()
        {
            using (var stream = new MemoryStream(new byte[100]))
            using (var reader = new EndianReader(stream))
            using (var writer = new EndianWriter(stream))
            {
                writer.Fill(22, 50);
                Assert.AreEqual(50, reader.BaseStream.Position);
                writer.Fill(44, 50);
                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                var buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));

                reader.Seek(0, SeekOrigin.Begin);
                writer.Copy(40, 60, 40);

                Assert.AreEqual(0, reader.BaseStream.Position); //must be preserved after copy

                reader.Seek(0, SeekOrigin.Begin);
                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(10);
                Assert.IsTrue(buffer.All(b => b == 44));

                buffer = reader.ReadBytes(10);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(30);
                Assert.IsTrue(buffer.All(b => b == 44));
            }
        }

        [TestMethod]
        public void TestCopy02()
        {
            using (var stream = new MemoryStream(new byte[100]))
            using (var reader = new EndianReader(stream))
            using (var writer = new EndianWriter(stream))
            {
                writer.Fill(22, 50);
                Assert.AreEqual(50, reader.BaseStream.Position);
                writer.Fill(44, 50);
                Assert.AreEqual(100, reader.BaseStream.Position);

                reader.Seek(0, SeekOrigin.Begin);
                var buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(50);
                Assert.IsTrue(buffer.All(b => b == 44));

                reader.Seek(0, SeekOrigin.Begin);
                writer.Copy(60, 40, 40);

                Assert.AreEqual(0, reader.BaseStream.Position); //must be preserved after copy

                reader.Seek(0, SeekOrigin.Begin);
                buffer = reader.ReadBytes(40);
                Assert.IsTrue(buffer.All(b => b == 22));

                buffer = reader.ReadBytes(40);
                Assert.IsTrue(buffer.All(b => b == 44));

                buffer = reader.ReadBytes(20);
                Assert.IsTrue(buffer.All(b => b == 44));
            }
        }
    }
}
