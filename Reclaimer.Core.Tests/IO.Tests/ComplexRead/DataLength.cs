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
        public void DataLength01(ByteOrder order, bool dynamicRead)
        {
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;

                writer.Write(5);
                writer.Write(100);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass14>();

                Assert.AreEqual(5, obj.Property1);
                Assert.AreEqual(100, obj.Property2);
                Assert.AreEqual(100, stream.Position);

                stream.Position = 0;
                writer.Write(7);
                writer.Write(45);

                stream.Position = 0;
                obj = reader.ReadObject<DataClass14>();

                Assert.AreEqual(7, obj.Property1);
                Assert.AreEqual(45, obj.Property2);
                Assert.AreEqual(45, stream.Position);
            }
        }

        public class DataClass14
        {
            [Offset(0x00)]
            public int Property1 { get; set; }

            [DataLength]
            [Offset(0x04)]
            public int Property2 { get; set; }
        }
    }
}
