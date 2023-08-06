namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void ByteOrder01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, ByteOrder.BigEndian))
            {
                var rand = new object[12];

                rand[0] = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue);
                writer.Seek(0x00, SeekOrigin.Begin);
                writer.Write((sbyte)rand[0]);

                rand[1] = (short)rng.Next(short.MinValue, short.MaxValue);
                writer.Seek(0x10, SeekOrigin.Begin);
                writer.Write((short)rand[1]);

                rand[2] = rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x20, SeekOrigin.Begin);
                writer.Write((int)rand[2]);

                rand[3] = (long)rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x30, SeekOrigin.Begin);
                writer.Write((long)rand[3], ByteOrder.LittleEndian);

                rand[4] = (byte)rng.Next(byte.MinValue, byte.MaxValue);
                writer.Seek(0x40, SeekOrigin.Begin);
                writer.Write((byte)rand[4]);

                rand[5] = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue);
                writer.Seek(0x50, SeekOrigin.Begin);
                writer.Write((ushort)rand[5]);

                rand[6] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x60, SeekOrigin.Begin);
                writer.Write((uint)rand[6]);

                rand[7] = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x70, SeekOrigin.Begin);
                writer.Write((ulong)rand[7]);

                rand[8] = (Half)rng.NextDouble();
                writer.Seek(0x80, SeekOrigin.Begin);
                writer.Write((Half)rand[8]);

                rand[9] = (float)rng.NextDouble();
                writer.Seek(0x90, SeekOrigin.Begin);
                writer.Write((float)rand[9]);

                rand[10] = rng.NextDouble();
                writer.Seek(0xA0, SeekOrigin.Begin);
                writer.Write((double)rand[10]);

                rand[11] = Guid.NewGuid();
                writer.Seek(0xB0, SeekOrigin.Begin);
                writer.Write((Guid)rand[11]);

                stream.Position = 0;
                var obj = reader.ReadObject<ByteOrderClass01>();

                Assert.AreEqual(0xFF, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.AreEqual(rand[2], obj.Property3);
                Assert.AreEqual(rand[3], obj.Property4);
                Assert.AreEqual(rand[4], obj.Property5);
                Assert.AreEqual(rand[5], obj.Property6);
                Assert.AreEqual(rand[6], obj.Property7);
                Assert.AreEqual(rand[7], obj.Property8);
                Assert.AreEqual(rand[8], obj.Property9);
                Assert.AreEqual(rand[9], obj.Property10);
                Assert.AreEqual(rand[10], obj.Property11);
                Assert.AreEqual(rand[11], obj.Property12);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void ByteOrder02(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[12];

                rand[0] = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue);
                writer.Seek(0x70, SeekOrigin.Begin);
                writer.Write((sbyte)rand[0]);

                rand[1] = (short)rng.Next(short.MinValue, short.MaxValue);
                writer.Seek(0x40, SeekOrigin.Begin);
                writer.Write((short)rand[1]);

                rand[2] = rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x30, SeekOrigin.Begin);
                writer.Write((int)rand[2]);

                rand[3] = (long)rng.Next(int.MinValue, int.MaxValue);
                writer.Seek(0x10, SeekOrigin.Begin);
                writer.Write((long)rand[3], ByteOrder.LittleEndian);

                rand[4] = (byte)rng.Next(byte.MinValue, byte.MaxValue);
                writer.Seek(0x90, SeekOrigin.Begin);
                writer.Write((byte)rand[4]);

                rand[5] = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue);
                writer.Seek(0xA0, SeekOrigin.Begin);
                writer.Write((ushort)rand[5]);

                rand[6] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x00, SeekOrigin.Begin);
                writer.Write((uint)rand[6]);

                rand[7] = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x80, SeekOrigin.Begin);
                writer.Write((ulong)rand[7], ByteOrder.BigEndian);

                rand[8] = (Half)rng.NextDouble();
                writer.Seek(0xB0, SeekOrigin.Begin);
                writer.Write((Half)rand[8]);

                rand[9] = (float)rng.NextDouble();
                writer.Seek(0x20, SeekOrigin.Begin);
                writer.Write((float)rand[9]);

                rand[10] = rng.NextDouble();
                writer.Seek(0x50, SeekOrigin.Begin);
                writer.Write((double)rand[10]);

                rand[11] = Guid.NewGuid();
                writer.Seek(0x60, SeekOrigin.Begin);
                writer.Write((Guid)rand[11]);

                stream.Position = 0;
                var obj = reader.ReadObject<ByteOrderClass02>();

                //the highest offset should always be read last
                //so if no size is specified the position should end
                //up at the highest offset + the size of the property
                Assert.AreEqual(0xB2, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.AreEqual(rand[2], obj.Property3);
                Assert.AreEqual(rand[3], obj.Property4);
                Assert.AreEqual(rand[4], obj.Property5);
                Assert.AreEqual(rand[5], obj.Property6);
                Assert.AreEqual(rand[6], obj.Property7);
                Assert.AreEqual(rand[7], obj.Property8);
                Assert.AreEqual(rand[8], obj.Property9);
                Assert.AreEqual(rand[9], obj.Property10);
                Assert.AreEqual(rand[10], obj.Property11);
                Assert.AreEqual(rand[11], obj.Property12);
            }
        }
    }
}
