using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestVirtualSeek
    {
        [TestMethod]
        public void VirtualReader()
        {
            using (var stream = new MemoryStream(new byte[1024]))
            using (var reader = new EndianReader(stream, ByteOrder.LittleEndian))
            {
                reader.Seek(50, SeekOrigin.Begin);
                Assert.AreEqual(50, stream.Position);

                reader.Seek(50, SeekOrigin.Current);
                Assert.AreEqual(100, stream.Position);

                reader.Seek(-50, SeekOrigin.End);
                Assert.AreEqual(974, stream.Position);

                using (var vreader = reader.CreateVirtualReader())
                {
                    vreader.Seek(0, SeekOrigin.Begin);
                    Assert.AreEqual(974, stream.Position);

                    vreader.ReadBytes(20);
                    Assert.AreEqual(994, stream.Position);
                }

                using (var vreader = reader.CreateVirtualReader(500))
                {
                    vreader.Seek(100, SeekOrigin.Begin);
                    Assert.AreEqual(600, stream.Position);
                }
            }
        }

        [TestMethod]
        public void VirtualWriter()
        {
            using (var stream = new MemoryStream(new byte[1024]))
            using (var writer = new EndianWriter(stream, ByteOrder.LittleEndian))
            {
                writer.Seek(50, SeekOrigin.Begin);
                Assert.AreEqual(50, stream.Position);

                writer.Seek(50, SeekOrigin.Current);
                Assert.AreEqual(100, stream.Position);

                writer.Seek(-50, SeekOrigin.End);
                Assert.AreEqual(974, stream.Position);

                using (var vwriter = writer.CreateVirtualWriter())
                {
                    vwriter.Seek(0, SeekOrigin.Begin);
                    Assert.AreEqual(974, stream.Position);

                    vwriter.Write(new byte[20]);
                    Assert.AreEqual(994, stream.Position);
                }

                using (var vwriter = writer.CreateVirtualWriter(500))
                {
                    vwriter.Seek(100, SeekOrigin.Begin);
                    Assert.AreEqual(600, stream.Position);
                }
            }
        }
    }
}
