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
        public void Enums01(ByteOrder order, bool dynamicRead)
        {
            var rng = new Random();
            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;
                var rand = new object[4];

                rand[0] = (Enum01)rng.Next(1, 4);
                writer.Write((byte)(Enum01)rand[0]);

                rand[1] = (Enum02)rng.Next(4, 7);
                writer.Write((short)(Enum02)rand[1]);

                rand[2] = (Enum03)rng.Next(7, 10);
                writer.Write((int)(Enum03)rand[2]);

                rand[3] = (Enum04)rng.Next(10, 13);
                writer.Write((long)(Enum04)rand[3]);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass12>();

                Assert.AreEqual((Enum01)rand[0], obj.Property1);
                Assert.AreEqual((Enum02)rand[1], obj.Property2);
                Assert.AreEqual((Enum03)rand[2], obj.Property3);
                Assert.AreEqual((Enum04)rand[3], obj.Property4);
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
