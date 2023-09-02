namespace Reclaimer.IO.Tests.DynamicWrite
{
    public partial class DynamicWrite
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian)]
        [DataRow(ByteOrder.BigEndian)]
        public void Enums01(ByteOrder order)
        {
            var rng = new Random();
            var obj = new DataClass12
            {
                Property1 = (Enum01)rng.Next(1, 4),
                Property2 = (Enum02)rng.Next(4, 7),
                Property3 = (Enum03)rng.Next(7, 10),
                Property4 = (Enum04)rng.Next(10, 13),
            };

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                writer.WriteObject(obj);

                stream.Position = 0;
                Assert.AreEqual((byte)obj.Property1, reader.ReadByte());
                Assert.AreEqual((short)obj.Property2, reader.ReadInt16());
                Assert.AreEqual((int)obj.Property3, reader.ReadInt32());
                Assert.AreEqual((long)obj.Property4, reader.ReadInt64());
            }
        }

        public class DataClass12
        {
            [Offset(0x00)]
            public Enum01 Property1 { get; set; }

            [Offset(0x01)]
            public Enum02 Property2 { get; set; }

            [Offset(0x03)]
            public Enum03 Property3 { get; set; }

            [Offset(0x07)]
            public Enum04 Property4 { get; set; }
        }

        public enum Enum01 : byte
        {
            Value01 = 1,
            Value02 = 2,
            Value03 = 3,
        }

        public enum Enum02 : short
        {
            Value01 = 4,
            Value02 = 5,
            Value03 = 6,
        }

        public enum Enum03
        {
            Value01 = 7,
            Value02 = 8,
            Value03 = 9,
        }

        public enum Enum04 : long
        {
            Value01 = 10,
            Value02 = 11,
            Value03 = 12,
        }
    }
}
