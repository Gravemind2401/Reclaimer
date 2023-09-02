using System.Text;

namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestString
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void LengthPrefixed(ByteOrder order)
        {
            var value1 = "Length_Prefixed_String_#01!";

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order, Encoding.UTF8))
            using (var writer = new EndianWriter(stream, order, Encoding.UTF8))
            {
                writer.Write(value1);
                Assert.AreEqual(value1.Length + 4, stream.Position);

                stream.Position = 0;
                var value2 = reader.ReadString();
                Assert.AreEqual(value1.Length + 4, stream.Position);
                Assert.AreEqual(value1, value2);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void FixedLength(ByteOrder order)
        {
            var value1 = "Fixed_Length_String_#01!";

            using (var stream = new MemoryStream(new byte[32]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteStringFixedLength(value1, 32);
                Assert.AreEqual(32, stream.Position);

                stream.Position = 0;
                var value2 = reader.ReadString(32, true);
                Assert.AreEqual(32, stream.Position);
                Assert.AreEqual(value1, value2);

                stream.Position = 0;
                value2 = reader.ReadString(32, false);
                Assert.AreEqual(32, value2.Length);
                Assert.IsTrue(value2.StartsWith(value1));
            }
        }
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void NullTerminated(ByteOrder order)
        {
            var value1 = "Null_Terminated_String_#01!";

            using (var stream = new MemoryStream(new byte[64]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteStringNullTerminated(value1);
                Assert.AreEqual(value1.Length + 1, stream.Position);

                stream.Position = 0;
                var value2 = reader.ReadNullTerminatedString();
                Assert.AreEqual(value1.Length + 1, stream.Position);
                Assert.AreEqual(value1, value2);

                stream.Position = 0;
                value2 = reader.ReadNullTerminatedString(64);
                Assert.AreEqual(64, stream.Position);
                Assert.AreEqual(value1, value2);
            }
        }
    }
}
