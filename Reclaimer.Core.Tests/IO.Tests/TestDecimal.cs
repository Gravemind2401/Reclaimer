using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestDecimal
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void DecimalSame(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rng = new Random();
                var rands = new decimal[4];

                for (int i = 0; i < 4; i++)
                    rands[i] = (decimal)(ulong.MaxValue * Math.Pow(rng.NextDouble(), 3));

                writer.Write(rands[0]);
                writer.Write(rands[1]);
                writer.Write(rands[2]);
                writer.Write(rands[3]);

                Assert.AreEqual(stream.Length, 64);

                stream.Position = 0;
                Assert.AreEqual(rands[0], reader.PeekDecimal());
                Assert.AreEqual(0, stream.Position);

                Assert.AreEqual(rands[0], reader.ReadDecimal());
                Assert.AreEqual(rands[1], reader.ReadDecimal());
                Assert.AreEqual(rands[2], reader.ReadDecimal());
                Assert.AreEqual(rands[3], reader.ReadDecimal());

                Assert.AreEqual(reader.BaseStream.Position, stream.Length);
            }
        }
    }
}
