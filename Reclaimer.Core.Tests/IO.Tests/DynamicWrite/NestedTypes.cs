namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Nested01(ByteOrder order)
        {
            var rng = new Random();
            int NextInt() => rng.Next(int.MinValue, int.MaxValue);

            var obj = new OuterClass01
            {
                Property1 = NextInt(),
                Property2 = new InnerClass01
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property3 = NextInt(),
                Property4 = new InnerStruct01
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property5 = NextInt()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property2, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property2, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property5, reader.ReadInt32());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Nested02(ByteOrder order)
        {
            var rng = new Random();
            int NextInt() => rng.Next(int.MinValue, int.MaxValue);

            var obj = new OuterStruct01
            {
                Property1 = NextInt(),
                Property2 = new InnerClass01
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property3 = NextInt(),
                Property4 = new InnerStruct01
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property5 = NextInt()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property2, reader.ReadInt32());
                Assert.AreEqual(obj.Property2.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property2, reader.ReadInt32());
                Assert.AreEqual(obj.Property4.Property3, reader.ReadInt32());
                Assert.AreEqual(obj.Property5, reader.ReadInt32());
            }
        }
    }
}
