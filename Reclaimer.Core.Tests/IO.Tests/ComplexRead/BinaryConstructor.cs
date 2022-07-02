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
        public void BinaryConstructor01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var value1 = rng.Next(int.MinValue, int.MaxValue);
                var value2 = (float)rng.NextDouble();
                var value3 = (byte)rng.Next(byte.MinValue, byte.MaxValue);

                writer.Write(value1);
                writer.Write(value2);
                writer.Write(value3);

                stream.Position = 0;
                var obj = reader.ReadObject<BinaryConstructorTest01>();

                Assert.AreEqual(value1, obj.Value1);
                Assert.AreEqual(value2, obj.Value2);
                Assert.AreEqual(value3, obj.Value3);

                stream.Position = 0;

                value1 = rng.Next(int.MinValue, int.MaxValue);
                value2 = (float)rng.NextDouble();
                value3 = (byte)rng.Next(byte.MinValue, byte.MaxValue);

                writer.Write(value1);
                writer.Write(value2);
                writer.Write(value3);

                stream.Position = 0;
                obj = reader.ReadObject<BinaryConstructorTest01>();

                Assert.AreEqual(value1, obj.Value1);
                Assert.AreEqual(value2, obj.Value2);
                Assert.AreEqual(value3, obj.Value3);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void BinaryConstructor02(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var value1 = rng.Next(int.MinValue, int.MaxValue);
                var value2 = (float)rng.NextDouble();
                var value3 = (byte)rng.Next(byte.MinValue, byte.MaxValue);

                writer.Write(value1);
                writer.Write(value2);
                writer.Write(value3);

                stream.Position = 0;
                var obj = reader.ReadObject<BinaryConstructorTest02>();

                Assert.AreEqual(value1, obj.Value1);
                Assert.AreEqual(value2, obj.Value2);
                Assert.AreEqual(value3, obj.Value3);

                stream.Position = 0;

                value1 = rng.Next(int.MinValue, int.MaxValue);
                value2 = (float)rng.NextDouble();
                value3 = (byte)rng.Next(byte.MinValue, byte.MaxValue);

                writer.Write(value1);
                writer.Write(value2);
                writer.Write(value3);

                stream.Position = 0;
                obj = reader.ReadObject<BinaryConstructorTest02>();

                Assert.AreEqual(value1, obj.Value1);
                Assert.AreEqual(value2, obj.Value2);
                Assert.AreEqual(value3, obj.Value3);
            }
        }
    }

    public class BinaryConstructorTest01
    {
        public int Value1 { get; set; }
        public float Value2 { get; set; }
        public byte Value3 { get; set; }

        [BinaryConstructor]
        public BinaryConstructorTest01(int value1, float value2, byte value3)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }
    }

    public struct BinaryConstructorTest02
    {
        public int Value1 { get; set; }
        public float Value2 { get; set; }
        public byte Value3 { get; set; }

        [BinaryConstructor]
        public BinaryConstructorTest02(int value1, float value2, byte value3)
        {
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }
    }
}
