namespace Reclaimer.IO.Tests.DynamicWrite
{
    [TestClass]
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_ClassBasic01(ByteOrder order)
        {
            Basic01<BasicClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_ClassBasic01(ByteOrder order)
        {
            Basic01<BasicClass01_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_ClassBasic02(ByteOrder order)
        {
            Basic02<BasicClass02>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_ClassBasic02(ByteOrder order)
        {
            Basic02<BasicClass02_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_StructBasic01(ByteOrder order)
        {
            Basic01<BasicStruct01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_StructBasic01(ByteOrder order)
        {
            Basic01<BasicStruct01_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_StructBasic02(ByteOrder order)
        {
            Basic02<BasicStruct02>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_StructBasic02(ByteOrder order)
        {
            Basic02<BasicStruct02_Builder>(order);
        }

        private static void Basic01<T>(ByteOrder order)
            where T : IBasicType, new()
        {
            var rng = new Random();
            var obj = new T
            {
                Property1 = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue),
                Property2 = (short)rng.Next(short.MinValue, short.MaxValue),
                Property3 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property4 = (long)rng.Next(int.MinValue, int.MaxValue),
                Property5 = (byte)rng.Next(byte.MinValue, byte.MaxValue),
                Property6 = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue),
                Property7 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property8 = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property9 = (Half)rng.NextDouble(),
                Property10 = (float)rng.NextDouble(),
                Property11 = (double)rng.NextDouble(),
                Property12 = Guid.NewGuid()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                Assert.AreEqual(0xFF, stream.Position);

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadSByte());

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadInt16());

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadInt32());

                reader.Seek(0x30, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property4, reader.ReadInt64());

                reader.Seek(0x40, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property5, reader.ReadByte());

                reader.Seek(0x50, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property6, reader.ReadUInt16());

                reader.Seek(0x60, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property7, reader.ReadUInt32());

                reader.Seek(0x70, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property8, reader.ReadUInt64());

                reader.Seek(0x80, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property9, reader.ReadHalf());

                reader.Seek(0x90, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property10, reader.ReadSingle());

                reader.Seek(0xA0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property11, reader.ReadDouble());

                reader.Seek(0xB0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property12, reader.ReadGuid());
            }
        }

        private static void Basic02<T>(ByteOrder order)
            where T : IBasicType, new()
        {
            var rng = new Random();
            var obj = new T
            {
                Property1 = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue),
                Property2 = (short)rng.Next(short.MinValue, short.MaxValue),
                Property3 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property4 = (long)rng.Next(int.MinValue, int.MaxValue),
                Property5 = (byte)rng.Next(byte.MinValue, byte.MaxValue),
                Property6 = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue),
                Property7 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property8 = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property9 = (Half)rng.NextDouble(),
                Property10 = (float)rng.NextDouble(),
                Property11 = (double)rng.NextDouble(),
                Property12 = Guid.NewGuid()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                //the highest offset should always be read last
                //so if no size is specified the position should end
                //up at the highest offset + the size of the property
                Assert.AreEqual(0xB2, stream.Position);

                reader.Seek(0x70, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadSByte());

                reader.Seek(0x40, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property2, reader.ReadInt16());

                reader.Seek(0x30, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property3, reader.ReadInt32());

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property4, reader.ReadInt64());

                reader.Seek(0x90, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property5, reader.ReadByte());

                reader.Seek(0xA0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property6, reader.ReadUInt16());

                reader.Seek(0x00, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property7, reader.ReadUInt32());

                reader.Seek(0x80, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property8, reader.ReadUInt64());

                reader.Seek(0xB0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property9, reader.ReadHalf());

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property10, reader.ReadSingle());

                reader.Seek(0x50, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property11, reader.ReadDouble());

                reader.Seek(0x60, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property12, reader.ReadGuid());
            }
        }
    }
}
