namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new VersionedClass01
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
            var obj = new VersionedClass02b
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
                Assert.AreEqual(1, reader.ReadInt32()); //version in stream must match version used to write
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.IsTrue(reader.ReadBytes(64).All(b => b == 0));

                stream.Position = 0;
                writer.Write(new byte[64]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 2);

                stream.Position = 0;
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
                Assert.AreEqual(2, reader.ReadInt32()); //version in stream must match version used to write
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
                Assert.AreEqual(3, reader.ReadInt32()); //version in stream must match version used to write
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
                Assert.AreEqual(4, reader.ReadInt32()); //version in stream must match version used to write
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property2, reader.ReadSingle());
                Assert.AreEqual(0, reader.ReadInt32());
                Assert.AreEqual(obj.Property4, reader.ReadDouble());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions03(ByteOrder order)
        {
            var rng = new Random();
            var obj = new VersionedClass03
            {
                Property1 = rng.Next(int.MinValue, int.MaxValue)
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj, 0);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x08, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 1);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x08, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 2);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x18, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 3);

                Assert.AreEqual(0x30, stream.Position);
                reader.Seek(0x18, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 4);

                Assert.AreEqual(0x30, stream.Position);
                reader.Seek(0x28, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 5);

                Assert.AreEqual(0x40, stream.Position);
                reader.Seek(0x28, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 6);

                Assert.AreEqual(0x40, stream.Position);
                reader.Seek(0x28, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions04(ByteOrder order)
        {
            var rng = new Random();
            var obj = new VersionedClass04
            {
                Property1 = rng.Next(int.MinValue, int.MaxValue)
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj, 0);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 1);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 2);

                Assert.AreEqual(0x20, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 3);

                Assert.AreEqual(0x30, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 4);

                Assert.AreEqual(0x30, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 5);

                Assert.AreEqual(0x40, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 6);

                Assert.AreEqual(0x40, stream.Position);
                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1, reader.ReadInt32());
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions05(ByteOrder order)
        {
            var rng = new Random();
            var obj = new VersionedClass05
            {
                Property1a = rng.Next(int.MinValue, int.MaxValue),
                Property1b = rng.Next(int.MinValue, int.MaxValue)
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj, 0);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1a, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 1);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1a, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 2);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1a, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 3);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1b, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 4);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1b, reader.ReadInt32());

                stream.Position = 0;
                writer.Write(new byte[0x50]); //set to zeros

                stream.Position = 0;
                writer.WriteObject(obj, 5);

                reader.Seek(0x10, SeekOrigin.Begin);
                Assert.AreEqual(obj.Property1b, reader.ReadInt32());
            }
        }
    }
}
