using System.Runtime.InteropServices;

namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
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
            where TOuter : class, IOuterType<TInnerClass, TInnerStruct>
            where TInnerClass : class, IInnerType
            where TInnerStruct : struct, IInnerType
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
                var obj = reader.ReadObject<TOuter>();

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

        private static void Nested02<TOuter, TInnerClass, TInnerStruct>(ByteOrder order)
            where TOuter : struct, IOuterType<TInnerClass, TInnerStruct>
            where TInnerClass : class, IInnerType
            where TInnerStruct : struct, IInnerType
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
