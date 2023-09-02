using System.Runtime.InteropServices;

namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions01(ByteOrder order)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
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
                var obj = (VersionedClass01)reader.ReadObject(typeof(VersionedClass01));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(1, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(2);
                stream.Position = 0;

                obj = (VersionedClass01)reader.ReadObject(typeof(VersionedClass01));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(2, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(3);
                stream.Position = 0;

                obj = (VersionedClass01)reader.ReadObject(typeof(VersionedClass01));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(3, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(4);
                stream.Position = 0;

                obj = (VersionedClass01)reader.ReadObject(typeof(VersionedClass01));

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
                var obj = (VersionedClass02a)reader.ReadObject(typeof(VersionedClass02a), 1);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (VersionedClass02a)reader.ReadObject(typeof(VersionedClass02a), 2);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (VersionedClass02a)reader.ReadObject(typeof(VersionedClass02a), 3);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (VersionedClass02a)reader.ReadObject(typeof(VersionedClass02a), 4);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.AreEqual(rand[4], obj.Property4);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions03(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[3];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x08, SeekOrigin.Begin);
                writer.Write(rand[0]);

                writer.Seek(0x18, SeekOrigin.Begin);
                writer.Write(rand[1]);

                writer.Seek(0x28, SeekOrigin.Begin);
                writer.Write(rand[2]);

                stream.Position = 0;
                var obj = reader.ReadObject<VersionedClass03>(0);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass03)reader.ReadObject(typeof(VersionedClass03), 1);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass03>(2);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[1], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass03)reader.ReadObject(typeof(VersionedClass03), 3);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[1], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass03>(4);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass03)reader.ReadObject(typeof(VersionedClass03), 5);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass03>(6);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions04(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[1];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x10, SeekOrigin.Begin);
                writer.Write(rand[0]);

                stream.Position = 0;
                var obj = reader.ReadObject<VersionedClass04>(0);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass04)reader.ReadObject(typeof(VersionedClass04), 1);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass04>(2);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass04)reader.ReadObject(typeof(VersionedClass04), 3);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass04>(4);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (VersionedClass04)reader.ReadObject(typeof(VersionedClass04), 5);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass04>(6);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Versions05(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[1];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x10, SeekOrigin.Begin);
                writer.Write(rand[0]);

                stream.Position = 0;
                var obj = reader.ReadObject<VersionedClass05>(0);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = (VersionedClass05)reader.ReadObject(typeof(VersionedClass05), 1);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass05>(2);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = (VersionedClass05)reader.ReadObject(typeof(VersionedClass05), 3);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);

                stream.Position = 0;
                obj = reader.ReadObject<VersionedClass05>(4);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);

                stream.Position = 0;
                obj = (VersionedClass05)reader.ReadObject(typeof(VersionedClass05), 5);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);
            }
        }
    }
}
