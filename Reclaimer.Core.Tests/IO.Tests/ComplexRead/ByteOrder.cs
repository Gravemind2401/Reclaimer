using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests.ComplexRead
{
    public partial class ComplexRead
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
                var rand = new object[10];

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

                rand[8] = (float)rng.NextDouble();
                writer.Seek(0x80, SeekOrigin.Begin);
                writer.Write((float)rand[8]);

                rand[9] = rng.NextDouble();
                writer.Seek(0x90, SeekOrigin.Begin);
                writer.Write((double)rand[9]);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass03>();

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
                var rand = new object[10];

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
                writer.Seek(0x60, SeekOrigin.Begin);
                writer.Write((ushort)rand[5]);

                rand[6] = unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x00, SeekOrigin.Begin);
                writer.Write((uint)rand[6]);

                rand[7] = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue));
                writer.Seek(0x80, SeekOrigin.Begin);
                writer.Write((ulong)rand[7], ByteOrder.BigEndian);

                rand[8] = (float)rng.NextDouble();
                writer.Seek(0x20, SeekOrigin.Begin);
                writer.Write((float)rand[8]);

                rand[9] = rng.NextDouble();
                writer.Seek(0x50, SeekOrigin.Begin);
                writer.Write((double)rand[9]);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass04>();

                //the highest offset should always be read last
                //so if no size is specified the position should end
                //up at the highest offset + the size of the property
                Assert.AreEqual(0x91, stream.Position);
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
            }
        }

        [FixedSize(0xFF)]
        [ByteOrder(ByteOrder.BigEndian)]
        public class DataClass03
        {
            [Offset(0x00)]
            public sbyte Property1 { get; set; }

            [Offset(0x10)]
            public short Property2 { get; set; }

            [Offset(0x20)]
            public int Property3 { get; set; }

            [Offset(0x30)]
            [ByteOrder(ByteOrder.LittleEndian)]
            public long Property4 { get; set; }

            [Offset(0x40)]
            public byte Property5 { get; set; }

            [Offset(0x50)]
            public ushort Property6 { get; set; }

            [Offset(0x60)]
            public uint Property7 { get; set; }

            [Offset(0x70)]
            public ulong Property8 { get; set; }

            [Offset(0x80)]
            public float Property9 { get; set; }

            [Offset(0x90)]
            public double Property10 { get; set; }
        }

        public class DataClass04
        {
            [Offset(0x70)]
            public sbyte Property1 { get; set; }

            [Offset(0x40)]
            public short Property2 { get; set; }

            [Offset(0x30)]
            public int Property3 { get; set; }

            [Offset(0x10)]
            [ByteOrder(ByteOrder.LittleEndian)]
            public long Property4 { get; set; }

            [Offset(0x90)]
            public byte Property5 { get; set; }

            [Offset(0x60)]
            public ushort Property6 { get; set; }

            [Offset(0x00)]
            public uint Property7 { get; set; }

            [Offset(0x80)]
            [ByteOrder(ByteOrder.BigEndian)]
            public ulong Property8 { get; set; }

            [Offset(0x20)]
            public float Property9 { get; set; }

            [Offset(0x50)]
            public double Property10 { get; set; }
        }
    }
}
