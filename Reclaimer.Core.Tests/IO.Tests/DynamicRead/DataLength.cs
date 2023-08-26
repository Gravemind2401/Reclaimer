﻿namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void DataLength01(ByteOrder order)
        {
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Write(5);
                writer.Write(100);

                stream.Position = 0;
                var obj = reader.ReadObject<DataLengthClass01>();

                Assert.AreEqual(5, obj.Property1);
                Assert.AreEqual(100, obj.Property2);
                Assert.AreEqual(100, stream.Position);

                stream.Position = 0;
                writer.Write(7);
                writer.Write(45);

                stream.Position = 0;
                obj = reader.ReadObject<DataLengthClass01>();

                Assert.AreEqual(7, obj.Property1);
                Assert.AreEqual(45, obj.Property2);
                Assert.AreEqual(45, stream.Position);
            }
        }
    }
}
