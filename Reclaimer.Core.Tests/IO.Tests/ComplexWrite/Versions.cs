namespace Reclaimer.IO.Tests.ComplexWrite
{
    public partial class ComplexWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new DataClass09
            {
                Version = 1,
                Property1 = rng.Next(int.MinValue, int.MaxValue),
                Property2 = (float)rng.NextDouble(),
                Property3 = (float)rng.NextDouble(),
                Property4 = rng.NextDouble(),
                Property5 = rng.NextDouble(),
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Version, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                obj.Version = 2;
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Version, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(obj.Property3, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                obj.Version = 3;
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Version, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(obj.Property3, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                obj.Version = 4;
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(obj.Version, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property4, reader.ReadDouble());
                Assert.AreEqual(obj.Property5, reader.ReadDouble());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions02(ByteOrder order)
        {
            var rng = new Random();
            var obj = new DataClass10
            {
                Version = 0,
                Property1 = rng.Next(int.MinValue, int.MaxValue),
                Property2 = (float)rng.NextDouble(),
                Property3 = (float)rng.NextDouble(),
                Property4 = rng.NextDouble()
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj, 1);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(1, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 2);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(2, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(obj.Property3, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 3);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(3, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(obj.Property3, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 4);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(4, reader.ReadInt32());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property4, reader.ReadDouble());
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
        }
    }
}
