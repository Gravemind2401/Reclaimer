namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Bufferable_Bufferable01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[3];

                rand[0] = rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x00, SeekOrigin.Begin);
                writer.Write((int)rand[0]);

                rand[1] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x04, SeekOrigin.Begin);
                writer.Write((uint)rand[1]);

                rand[2] = (float)rng.NextDouble();
                writer.Seek(0x08, SeekOrigin.Begin);
                writer.Write((float)rand[2]);

                stream.Position = 0;
                var obj = reader.ReadBufferable<BufferableStruct01>();

                Assert.AreEqual(BufferableStruct01.SizeOf, stream.Position);
                Assert.AreEqual(obj.Property1, rand[0]);
                Assert.AreEqual(obj.Property2, rand[1]);
                Assert.AreEqual(obj.Property3, rand[2]);

                stream.Position = 0;
                obj = reader.ReadObject<BufferableStruct01>();

                Assert.AreEqual(BufferableStruct01.SizeOf, stream.Position);
                Assert.AreEqual(obj.Property1, rand[0]);
                Assert.AreEqual(obj.Property2, rand[1]);
                Assert.AreEqual(obj.Property3, rand[2]);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Bufferable02(ByteOrder order)
        {
            Bufferable02<ClassWithBufferable01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Bufferable02(ByteOrder order)
        {
            Bufferable02<ClassWithBufferable01_Builder>(order);
        }

        private static void Bufferable02<T>(ByteOrder order)
            where T : ClassWithBufferable01
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[5];

                rand[0] = rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x00, SeekOrigin.Begin);
                writer.Write((int)rand[0]);

                rand[1] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x10, SeekOrigin.Begin);
                writer.Write((uint)rand[1]);

                rand[2] = rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x20, SeekOrigin.Begin);
                writer.Write((int)rand[2]);

                rand[3] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x24, SeekOrigin.Begin);
                writer.Write((uint)rand[3]);

                rand[4] = (float)rng.NextDouble();
                writer.Seek(0x28, SeekOrigin.Begin);
                writer.Write((float)rand[4]);

                stream.Position = 0;
                var obj = reader.ReadObject<T>();

                Assert.AreEqual(0x20 + BufferableStruct01.SizeOf, stream.Position);
                Assert.AreEqual(obj.Property1, rand[0]);
                Assert.AreEqual(obj.Property2, rand[1]);
                Assert.AreEqual(obj.Property3.Property1, rand[2]);
                Assert.AreEqual(obj.Property3.Property2, rand[3]);
                Assert.AreEqual(obj.Property3.Property3, rand[4]);
            }
        }
    }
}
