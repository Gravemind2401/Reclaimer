namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Nested01(ByteOrder order)
        {
            Nested01<OuterClass01, InnerClass01, InnerStruct01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Nested01(ByteOrder order)
        {
            Nested01<OuterClass01_Builder, InnerClass01_Builder, InnerStruct01_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Nested02(ByteOrder order)
        {
            Nested02<OuterStruct01, InnerClass01, InnerStruct01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Nested02(ByteOrder order)
        {
            Nested02<OuterStruct01_Builder, InnerClass01_Builder, InnerStruct01_Builder>(order);
        }

        private static void Nested01<TOuter, TInnerClass, TInnerStruct>(ByteOrder order)
            where TOuter : class, IOuterType<TInnerClass, TInnerStruct>, new()
            where TInnerClass : class, IInnerType, new()
            where TInnerStruct : struct, IInnerType
        {
            var rng = new Random();
            int NextInt() => rng.Next(int.MinValue, int.MaxValue);

            var obj = new TOuter
            {
                Property1 = NextInt(),
                Property2 = new TInnerClass
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property3 = NextInt(),
                Property4 = new TInnerStruct
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

        private static void Nested02<TOuter, TInnerClass, TInnerStruct>(ByteOrder order)
            where TOuter : struct, IOuterType<TInnerClass, TInnerStruct>
            where TInnerClass : class, IInnerType, new()
            where TInnerStruct : struct, IInnerType
        {
            var rng = new Random();
            int NextInt() => rng.Next(int.MinValue, int.MaxValue);

            var obj = new TOuter
            {
                Property1 = NextInt(),
                Property2 = new TInnerClass
                {
                    Property1 = NextInt(),
                    Property2 = NextInt(),
                    Property3 = NextInt()
                },
                Property3 = NextInt(),
                Property4 = new TInnerStruct
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
