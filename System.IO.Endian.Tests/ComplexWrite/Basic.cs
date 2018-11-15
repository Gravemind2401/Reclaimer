using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests.ComplexWrite
{
    [TestClass]
    public partial class ComplexWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Basic01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new DataClass01
            {
                Property1 = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue),
                Property2 = (short)rng.Next(short.MinValue, short.MaxValue),
                Property3 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property4 = (long)rng.Next(int.MinValue, int.MaxValue),
                Property5 = (byte)rng.Next(byte.MinValue, byte.MaxValue),
                Property6 = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue),
                Property7 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property8 = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property9 = (float)rng.NextDouble(),
                Property10 = (double)rng.NextDouble(),
                Property11 = Guid.NewGuid()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {

                writer.WriteComplex(obj);

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
                Assert.AreEqual(obj.Property9, reader.ReadSingle());

                reader.Seek(0x90, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property10, reader.ReadDouble());

                reader.Seek(0xA0, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property11, reader.ReadGuid());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Basic02(ByteOrder order)
        {
            var rng = new Random();
            var obj = new DataClass02
            {
                Property1 = (sbyte)rng.Next(sbyte.MinValue, sbyte.MaxValue),
                Property2 = (short)rng.Next(short.MinValue, short.MaxValue),
                Property3 = (int)rng.Next(int.MinValue, int.MaxValue),
                Property4 = (long)rng.Next(int.MinValue, int.MaxValue),
                Property5 = (byte)rng.Next(byte.MinValue, byte.MaxValue),
                Property6 = (ushort)rng.Next(ushort.MinValue, ushort.MaxValue),
                Property7 = unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property8 = (ulong)unchecked((uint)rng.Next(int.MinValue, int.MaxValue)),
                Property9 = (float)rng.NextDouble(),
                Property10 = (double)rng.NextDouble(),
                Property11 = Guid.NewGuid()
            };

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteComplex(obj);

                //the highest offset should always be read last
                //so if no size is specified the position should end
                //up at the highest offset + the size of the property
                Assert.AreEqual(0xA2, stream.Position);

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

                reader.Seek(0x20, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property9, reader.ReadSingle());

                reader.Seek(0x50, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property10, reader.ReadDouble());

                reader.Seek(0x60, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property11, reader.ReadGuid());
            }
        }

        [ObjectSize(0xFF)]
        public class DataClass01
        {
            [Offset(0x00)]
            public sbyte Property1 { get; set; }

            [Offset(0x10)]
            public short Property2 { get; set; }

            [Offset(0x20)]
            public int Property3 { get; set; }

            [Offset(0x30)]
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

            [Offset(0xA0)]
            public Guid Property11 { get; set; }
        }

        public class DataClass02
        {
            [Offset(0x70)]
            public sbyte Property1 { get; set; }

            [Offset(0x40)]
            public short Property2 { get; set; }

            [Offset(0x30)]
            public int Property3 { get; set; }

            [Offset(0x10)]
            public long Property4 { get; set; }

            [Offset(0x90)]
            public byte Property5 { get; set; }

            [Offset(0xA0)]
            public ushort Property6 { get; set; }

            [Offset(0x00)]
            public uint Property7 { get; set; }

            [Offset(0x80)]
            public ulong Property8 { get; set; }

            [Offset(0x20)]
            public float Property9 { get; set; }

            [Offset(0x50)]
            public double Property10 { get; set; }

            [Offset(0x60)]
            public Guid Property11 { get; set; }
        }
    }
}
