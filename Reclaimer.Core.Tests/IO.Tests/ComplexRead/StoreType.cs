using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests.ComplexRead
{
    public partial class ComplexRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, false)]
        [DataRow(ByteOrder.BigEndian, false)]
        [DataRow(ByteOrder.LittleEndian, true)]
        [DataRow(ByteOrder.BigEndian, true)]
        public void StoreType01(ByteOrder order, bool dynamicRead)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;
                var rand = new object[3];

                rand[0] = (short)rng.Next(short.MinValue, short.MaxValue);
                writer.Write((short)rand[0]);

                rand[1] = (byte)rng.Next(byte.MinValue, byte.MaxValue);
                writer.Write((byte)rand[1]);

                rand[2] = (float)rng.NextDouble();
                writer.Write((float)rand[2]);

                stream.Position = 0;
                var obj = (DataClass11)reader.ReadObject(typeof(DataClass11));

                Assert.AreEqual(rand[0], (short)obj.Property1);
                Assert.AreEqual(rand[1], (byte)obj.Property2);
                Assert.AreEqual(rand[2], (float)obj.Property3);
            }
        }

        public class DataClass11
        {
            [Offset(0x00)]
            [StoreType(typeof(short))]
            public int Property1 { get; set; }

            [Offset(0x02)]
            [StoreType(typeof(byte))]
            public int Property2 { get; set; }

            [Offset(0x03)]
            [StoreType(typeof(float))]
            public double Property3 { get; set; }
        }
    }
}
