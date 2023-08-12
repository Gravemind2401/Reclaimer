using System.Runtime.InteropServices;

namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Nested01(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[9];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x00, SeekOrigin.Begin);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                var obj = reader.ReadObject<OuterClass01>();

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(rand[1], obj.Property2.Property1);
                Assert.AreEqual(rand[2], obj.Property2.Property2);
                Assert.AreEqual(rand[3], obj.Property2.Property3);
                Assert.AreEqual(rand[4], obj.Property3);
                Assert.AreEqual(rand[5], obj.Property4.Property1);
                Assert.AreEqual(rand[6], obj.Property4.Property2);
                Assert.AreEqual(rand[7], obj.Property4.Property3);
                Assert.AreEqual(rand[8], obj.Property5);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Nested02(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[9];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x00, SeekOrigin.Begin);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                var obj = reader.ReadObject<OuterStruct01>();

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(rand[1], obj.Property2.Property1);
                Assert.AreEqual(rand[2], obj.Property2.Property2);
                Assert.AreEqual(rand[3], obj.Property2.Property3);
                Assert.AreEqual(rand[4], obj.Property3);
                Assert.AreEqual(rand[5], obj.Property4.Property1);
                Assert.AreEqual(rand[6], obj.Property4.Property2);
                Assert.AreEqual(rand[7], obj.Property4.Property3);
                Assert.AreEqual(rand[8], obj.Property5);
            }
        }
    }
}
