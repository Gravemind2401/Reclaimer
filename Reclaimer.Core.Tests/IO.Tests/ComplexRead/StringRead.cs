using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Tests.ComplexRead
{
    public partial class ComplexRead
    {
        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, false)]
        [DataRow(ByteOrder.BigEndian, false)]
        [DataRow(ByteOrder.LittleEndian, true)]
        [DataRow(ByteOrder.BigEndian, true)]
        public void ReadStrings01(ByteOrder order, bool dynamicRead)
        {
            var value1 = "Length_Prefixed_String_#01!";
            var value2 = "Fixed_Length_String_#01!";
            var value3 = "Null_Terminated_String_#01!";

            using (var stream = new MemoryStream(new byte[500]))
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;
                writer.Write(value1);

                stream.Position = 0x20;
                writer.WriteStringFixedLength(value2, 32);

                stream.Position = 0x40;
                writer.WriteStringNullTerminated(value3);

                stream.Position = 0x60;
                writer.Write(value1, ByteOrder.LittleEndian);

                stream.Position = 0x80;
                writer.Write(value1, ByteOrder.BigEndian);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass05>();

                Assert.AreEqual(value1, obj.Property1);

                Assert.AreEqual(value2, obj.Property2);

                Assert.AreEqual(32, obj.Property3.Length);
                Assert.IsTrue(obj.Property3.StartsWith(value2));

                Assert.AreEqual(value3, obj.Property4);
                Assert.AreEqual(value3, obj.Property5);

                Assert.AreEqual(value1, obj.Property6);
                Assert.AreEqual(value1, obj.Property7);
            }
        }

        [DataTestMethod]
        [DataRow(ByteOrder.LittleEndian, false)]
        [DataRow(ByteOrder.BigEndian, false)]
        [DataRow(ByteOrder.LittleEndian, true)]
        [DataRow(ByteOrder.BigEndian, true)]
        public void ReadStrings02(ByteOrder order, bool dynamicRead)
        {
            var value1 = "Length_Prefixed_String_#01!";
            var value2 = "Length_Prefixed_String_#02!";
            var value3 = "Length_Prefixed_String_#03!";

            using (var stream = new MemoryStream())
            using (var reader = new EndianReader(stream, order))
            using (var writer = new EndianWriter(stream, order))
            {
                reader.DynamicReadEnabled = dynamicRead;
                writer.Write(value1, ByteOrder.BigEndian);

                stream.Position = 0x20;
                writer.Write(value2, ByteOrder.LittleEndian);

                stream.Position = 0x40;
                writer.Write(value3, ByteOrder.BigEndian);

                stream.Position = 0;
                var obj = reader.ReadObject<DataClass06>();

                Assert.AreEqual(value1, obj.Property1);
                Assert.AreEqual(value2, obj.Property2);
                Assert.AreEqual(value3, obj.Property3);
            }
        }

        public class DataClass05
        {
            [Offset(0x00)]
            [LengthPrefixed]
            public string Property1 { get; set; }

            [Offset(0x20)]
            [FixedLength(32, Trim = true)]
            public string Property2 { get; set; }

            [Offset(0x20)]
            [FixedLength(32)]
            public string Property3 { get; set; }

            [Offset(0x40)]
            [NullTerminated]
            public string Property4 { get; set; }

            [Offset(0x40)]
            [NullTerminated(Length = 64)]
            public string Property5 { get; set; }

            [Offset(0x60)]
            [LengthPrefixed]
            [ByteOrder(ByteOrder.LittleEndian)]
            public string Property6 { get; set; }

            [Offset(0x80)]
            [LengthPrefixed]
            [ByteOrder(ByteOrder.BigEndian)]
            public string Property7 { get; set; }
        }

        [ByteOrder(ByteOrder.BigEndian)]
        public class DataClass06
        {
            [Offset(0x00)]
            [LengthPrefixed]
            public string Property1 { get; set; }

            [Offset(0x20)]
            [LengthPrefixed]
            [ByteOrder(ByteOrder.LittleEndian)]
            public string Property2 { get; set; }

            [Offset(0x40)]
            [LengthPrefixed]
            [ByteOrder(ByteOrder.BigEndian)]
            public string Property3 { get; set; }
        }
    }
}
