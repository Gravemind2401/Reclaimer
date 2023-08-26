namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Bufferable_Bufferable01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new BufferableStruct01
            {
                Property1 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property2 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property3 = (float)rng.NextDouble()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteBufferable(obj);

                Assert.AreEqual(BufferableStruct01.SizeOf, stream.Position);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                reader.Seek(0x04, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadUInt32());

                reader.Seek(0x08, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadSingle());

                stream.Position = 0;
                writer.WriteObject(obj);

                Assert.AreEqual(BufferableStruct01.SizeOf, stream.Position);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                reader.Seek(0x04, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadUInt32());

                reader.Seek(0x08, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadSingle());
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
            where T : ClassWithBufferable01, new()
        {
            var rng = new Random();
            var obj = new T
            {
                Property1 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property2 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property3 = new BufferableStruct01
                {
                    Property1 = (int)rng.Next(int.MinValue, int.MaxValue),
                    Property2 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                    Property3 = (float)rng.NextDouble(),
                }
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                Assert.AreEqual(0x20 + BufferableStruct01.SizeOf, stream.Position);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadUInt32());

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3.Property1, reader.ReadInt32());

                reader.Seek(0x24, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3.Property2, reader.ReadUInt32());

                reader.Seek(0x28, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3.Property3, reader.ReadSingle());
            }
        }
    }
}
