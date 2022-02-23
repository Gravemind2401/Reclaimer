using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests.ComplexRead
{
    public partial class ComplexRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, false)]
        [DataRow(ByteOrder.BigEndian, false)]
        [DataRow(ByteOrder.LittleEndian, true)]
        [DataRow(ByteOrder.BigEndian, true)]
        public void Versions01(ByteOrder order, bool dynamicRead)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;

                var rand = new object[6];

                rand[0] = rng.Next(int.MinValue, int.MaxValue);
                writer.Write((int)rand[0]);

                writer.Write(1);

                rand[1] = (float)rng.NextDouble();
                writer.Write((float)rand[1]);

                rand[2] = (float)rng.NextDouble();
                writer.Write((float)rand[2]);

                rand[3] = (float)rng.NextDouble();
                writer.Write((float)rand[3]);

                rand[4] = rng.NextDouble();
                writer.Write((double)rand[4]);

                rand[5] = rng.NextDouble();
                writer.Write((double)rand[5]);

                stream.Position = 0;
                var obj = (DataClass09)reader.ReadObject(typeof(DataClass09));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(1, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(2);
                stream.Position = 0;

                obj = (DataClass09)reader.ReadObject(typeof(DataClass09));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(2, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(3);
                stream.Position = 0;

                obj = (DataClass09)reader.ReadObject(typeof(DataClass09));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(3, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(4);
                stream.Position = 0;

                obj = (DataClass09)reader.ReadObject(typeof(DataClass09));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(4, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.AreEqual(rand[4], obj.Property4);
                Assert.AreEqual(rand[5], obj.Property5);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions02(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                var rand = new object[5];

                rand[0] = rng.Next(int.MinValue, int.MaxValue);
                writer.Write((int)rand[0]);

                writer.Write(0);

                rand[1] = (float)rng.NextDouble();
                writer.Write((float)rand[1]);

                rand[2] = (float)rng.NextDouble();
                writer.Write((float)rand[2]);

                rand[3] = (float)rng.NextDouble();
                writer.Write((float)rand[3]);

                rand[4] = rng.NextDouble();
                writer.Write((double)rand[4]);

                stream.Position = 0;
                var obj = (DataClass10)reader.ReadObject(typeof(DataClass10), 1);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (DataClass10)reader.ReadObject(typeof(DataClass10), 2);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (DataClass10)reader.ReadObject(typeof(DataClass10), 3);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (DataClass10)reader.ReadObject(typeof(DataClass10), 4);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.AreEqual(rand[4], obj.Property4);
            }
        }

        public class DataClass09
        {
            [Offset(0x00)]
            public int Property1 { get; set; }

            [Offset(0x04)]
            [VersionNumber]
            public int Version { get; set; }

            [Offset(0x08, MaxVersion = 2)]
            [Offset(0x0C, MinVersion = 2)]
            public float Property2 { get; set; }

            [Offset(0x10)]
            [MinVersion(2)]
            [MaxVersion(4)]
            public float? Property3 { get; set; }

            [Offset(0x14)]
            [VersionSpecific(4)]
            public double? Property4 { get; set; }

            [Offset(0x1C)]
            [MinVersion(4)]
            [MaxVersion(4)]
            public double? Property5 { get; set; }
        }

        public class DataClass10
        {
            [Offset(0x00)]
            public int Property1 { get; set; }

            [Offset(0x04)]
            public int Version { get; set; }

            [Offset(0x08, MaxVersion = 2)]
            [Offset(0x0C, MinVersion = 2)]
            public float Property2 { get; set; }

            [Offset(0x10)]
            [MinVersion(2)]
            [MaxVersion(4)]
            public float? Property3 { get; set; }

            [Offset(0x14)]
            [VersionSpecific(4)]
            public double? Property4 { get; set; }
        }
    }
}
