using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestEnumerable
    {
        private static int[] GetRandom(int count)
        {
            var rng = new Random();
            var result = new int[count];
            for (int i = 0; i < count; i++)
                result[i] = rng.Next(int.MinValue, int.MaxValue);
            return result;
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void ReadEnumerable(ByteOrder order)
        {
            using (var stream = new MemoryStream(new byte[1024]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = GetRandom(100);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                int index = 0;
                var enumerable = reader.ReadEnumerable<int>(100).ToArray();
                Assert.AreEqual(100, enumerable.Length);

                foreach (var i in enumerable)
                    Assert.AreEqual(rand[index++], i);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void WriteEnumerable(ByteOrder order)
        {
            using (var stream = new MemoryStream(new byte[1024]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = GetRandom(100);
                writer.WriteEnumerable(rand);

                stream.Position = 0;
                for (int i = 0; i < 100; i++)
                    Assert.AreEqual(rand[i], reader.ReadInt32());
            }
        }
    }
}
