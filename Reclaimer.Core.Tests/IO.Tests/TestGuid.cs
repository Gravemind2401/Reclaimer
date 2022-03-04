using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestGuid
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void GuidSame(ByteOrder order)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var guid1 = Guid.NewGuid();
                var guid2 = Guid.NewGuid();

                writer.Write(guid1);
                writer.Write(guid2);

                writer.Write(guid1, ByteOrder.LittleEndian);
                writer.Write(guid2, ByteOrder.LittleEndian);

                writer.Write(guid1, ByteOrder.BigEndian);
                writer.Write(guid2, ByteOrder.BigEndian);

                stream.Position = 0;

                Assert.AreEqual(guid1, reader.ReadGuid());
                Assert.AreEqual(guid2, reader.ReadGuid());

                Assert.AreEqual(guid1, reader.ReadGuid(ByteOrder.LittleEndian));
                Assert.AreEqual(guid2, reader.ReadGuid(ByteOrder.LittleEndian));

                Assert.AreEqual(guid1, reader.ReadGuid(ByteOrder.BigEndian));
                Assert.AreEqual(guid2, reader.ReadGuid(ByteOrder.BigEndian));
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, ByteOrder.BigEndian)]
        [DataRow(ByteOrder.BigEndian, ByteOrder.LittleEndian)]
        public void GuidMixed(ByteOrder readOrder, ByteOrder writeOrder)
        {
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, readOrder))
            using (var writer = new EndianWriter(stream, writeOrder))
            {
                var guidX = new Guid("863b519c-6942-4b65-adfa-55874889fecd");
                var guidY = new Guid("9c513b86-4269-654b-adfa-55874889fecd");

                writer.Write(guidX, ByteOrder.LittleEndian);
                writer.Write(guidY, ByteOrder.LittleEndian);

                writer.Write(guidX, ByteOrder.BigEndian);
                writer.Write(guidY, ByteOrder.BigEndian);

                stream.Position = 0;

                Assert.AreEqual(guidY, reader.ReadGuid(ByteOrder.BigEndian));
                Assert.AreEqual(guidX, reader.ReadGuid(ByteOrder.BigEndian));

                Assert.AreEqual(guidY, reader.ReadGuid(ByteOrder.LittleEndian));
                Assert.AreEqual(guidX, reader.ReadGuid(ByteOrder.LittleEndian));
            }
        }
    }
}
