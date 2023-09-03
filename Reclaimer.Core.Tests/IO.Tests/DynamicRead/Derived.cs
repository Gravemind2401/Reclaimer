using System.Runtime.InteropServices;

namespace Reclaimer.IO.Tests.DynamicRead
{
    public partial class DynamicRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Derived01(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[2];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new DerivedReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x00, SeekOrigin.Begin);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                var obj = reader.ReadObject<DerivedTestClass01>();

                //if no properties with [Offset] and no [FixedSize] then the position must remain where the derived class left it
                //ie we must assume the derived class handled the reading and left the position at the end of the object
                Assert.AreEqual(sizeof(int), stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);

                stream.Position = 0;
                obj = (DerivedTestClass01)reader.ReadObject(typeof(DerivedTestClass01));

                //if no properties with [Offset] and no [FixedSize] then the position must remain where the derived class left it
                //ie we must assume the derived class handled the reading and left the position at the end of the object
                Assert.AreEqual(sizeof(int), stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Derived02(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[2];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new DerivedReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x00, SeekOrigin.Begin);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                var obj = reader.ReadObject<DerivedTestClass02>();

                //derived class read an int but there was another property, so position must remain at the end of the highest offset + sizeof property
                Assert.AreEqual(sizeof(int) * 2, stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);
                Assert.AreEqual(rand[1], obj.Property1);

                stream.Position = 0;
                obj = (DerivedTestClass02)reader.ReadObject(typeof(DerivedTestClass02));

                //derived class read an int but there was another property, so position must remain at the end of the highest offset + sizeof property
                Assert.AreEqual(sizeof(int) * 2, stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);
                Assert.AreEqual(rand[1], obj.Property1);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Derived03(ByteOrder order)
        {
            var rng = new Random();
            var rand = new int[2];
            rng.NextBytes(MemoryMarshal.AsBytes<int>(rand));

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new DerivedReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.Seek(0x00, SeekOrigin.Begin);
                foreach (var i in rand)
                    writer.Write(i);

                stream.Position = 0;
                var obj = reader.ReadObject<DerivedTestClass03>();

                //position must end up at the size defined by [FixedSize]
                Assert.AreEqual(0x10, stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);

                stream.Position = 0;
                obj = (DerivedTestClass03)reader.ReadObject(typeof(DerivedTestClass03));

                //position must end up at the size defined by [FixedSize]
                Assert.AreEqual(0x10, stream.Position);
                Assert.AreEqual(rand[0], obj.Property0);
            }
        }
    }
}
