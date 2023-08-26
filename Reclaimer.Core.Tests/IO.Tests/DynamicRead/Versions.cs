using System.Runtime.InteropServices;

namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Versions01(ByteOrder order)
        {
            Versions01<VersionedClass01>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Versions02(ByteOrder order)
        {
            Versions02<VersionedClass02a>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Versions03(ByteOrder order)
        {
            Versions03<VersionedClass03>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Versions04(ByteOrder order)
        {
            Versions04<VersionedClass04>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Attributes_Versions05(ByteOrder order)
        {
            Versions05<VersionedClass05>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Versions01(ByteOrder order)
        {
            Versions01<VersionedClass01_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Versions02(ByteOrder order)
        {
            Versions02<VersionedClass02a_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Versions03(ByteOrder order)
        {
            Versions03<VersionedClass03_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Versions04(ByteOrder order)
        {
            Versions04<VersionedClass04_Builder>(order);
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Builder_Versions05(ByteOrder order)
        {
            Versions05<VersionedClass05_Builder>(order);
        }

        private static void Versions01<T>(ByteOrder order)
            where T : VersionedClass01
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
                var obj = (T)reader.ReadObject(typeof(T));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(1, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(2);
                stream.Position = 0;

                obj = (T)reader.ReadObject(typeof(T));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(2, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(3);
                stream.Position = 0;

                obj = (T)reader.ReadObject(typeof(T));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(3, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 4;
                writer.Write(4);
                stream.Position = 0;

                obj = (T)reader.ReadObject(typeof(T));

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(4, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.AreEqual(rand[4], obj.Property4);
                Assert.AreEqual(rand[5], obj.Property5);
            }
        }

        private static void Versions02<T>(ByteOrder order)
            where T : VersionedClass02a
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
                var obj = (T)reader.ReadObject(typeof(T), 1);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[1], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 2);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 3);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.AreEqual(rand[3], obj.Property3);
                Assert.IsNull(obj.Property4);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 4);

                Assert.AreEqual(rand[0], obj.Property1);
                Assert.AreEqual(0, obj.Version);
                Assert.AreEqual(rand[2], obj.Property2);
                Assert.IsNull(obj.Property3);
                Assert.AreEqual(rand[4], obj.Property4);
            }
        }

        private static void Versions03<T>(ByteOrder order)
            where T : VersionedClass03
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
                var obj = reader.ReadObject<T>(0);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 1);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(2);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[1], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 3);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[1], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(4);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 5);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(6);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[2], obj.Property1);
            }
        }

        private static void Versions04<T>(ByteOrder order)
            where T : VersionedClass04
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
                var obj = reader.ReadObject<T>(0);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 1);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(2);

                Assert.AreEqual(0x20, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 3);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(4);

                Assert.AreEqual(0x30, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 5);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);

                stream.Position = 0;
                obj = reader.ReadObject<T>(6);

                Assert.AreEqual(0x40, stream.Position);
                Assert.AreEqual(rand[0], obj.Property1);
            }
        }

        private static void Versions05<T>(ByteOrder order)
            where T : VersionedClass05
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
                var obj = reader.ReadObject<T>(0);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 1);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = reader.ReadObject<T>(2);

                Assert.AreEqual(rand[0], obj.Property1a);
                Assert.IsNull(obj.Property1b);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 3);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);

                stream.Position = 0;
                obj = reader.ReadObject<T>(4);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);

                stream.Position = 0;
                obj = (T)reader.ReadObject(typeof(T), 5);

                Assert.IsNull(obj.Property1a);
                Assert.AreEqual(rand[0], obj.Property1b);
            }
        }
    }
}
