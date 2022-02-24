using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reclaimer.IO.Tests.ComplexWrite
{
    public partial class ComplexWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void DataLength01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var obj = new DataClass14
                {
                    Property1 = 5,
                    Property2 = 100
                };

                writer.WriteObject(obj);
                
                Assert.AreEqual(100, stream.Position);
                stream.Position = 0;
                Assert.AreEqual(5, reader.ReadInt32());
                Assert.AreEqual(100, reader.ReadInt32());

                stream.Position = 0;
                obj = new DataClass14
                {
                    Property1 = 7,
                    Property2 = 45
                };

                writer.WriteObject(obj);

                Assert.AreEqual(45, stream.Position);
                stream.Position = 0;
                Assert.AreEqual(7, reader.ReadInt32());
                Assert.AreEqual(45, reader.ReadInt32());
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
