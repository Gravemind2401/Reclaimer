using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.IO.Endian.Tests
{
    [TestClass]
    public class TestAnnotations
    {
        [TestMethod]
        public void TestOffsetValue()
        {
            var prop1 = typeof(DataClass01).GetProperty(nameof(DataClass01.Property1));
            var prop2 = typeof(DataClass01).GetProperty(nameof(DataClass01.Property2));
            var prop3 = typeof(DataClass01).GetProperty(nameof(DataClass01.Property3));

            Assert.AreEqual(0, OffsetAttribute.ValueFor(prop1));

            Assert.AreEqual(10, OffsetAttribute.ValueFor(prop2, 15));
            Assert.AreEqual(15, OffsetAttribute.ValueFor(prop2, 25));

            Assert.AreEqual(20, OffsetAttribute.ValueFor(prop3, 15));
        }

        [TestMethod]
        public void TestOffsetValueExpression()
        {
            Assert.AreEqual(0, OffsetAttribute.ValueFor((DataClass01 obj) => obj.Property1));

            Assert.AreEqual(10, OffsetAttribute.ValueFor((DataClass01 obj) => obj.Property2, 15));
            Assert.AreEqual(15, OffsetAttribute.ValueFor((DataClass01 obj) => obj.Property2, 25));

            Assert.AreEqual(20, OffsetAttribute.ValueFor((DataClass01 obj) => obj.Property3, 15));
        }

        [TestMethod]
        public void TestFixedSizeValue()
        {
            Assert.AreEqual(128, FixedSizeAttribute.ValueFor(typeof(DataClass01)));
            Assert.AreEqual(256, FixedSizeAttribute.ValueFor(typeof(DataClass01), 15));
            Assert.AreEqual(512, FixedSizeAttribute.ValueFor(typeof(DataClass01), 25));
        }

        [FixedSize(128)]
        [FixedSize(256, MinVersion = 10, MaxVersion = 20)]
        [FixedSize(512, MinVersion = 20)]
        public class DataClass01
        {
            [Offset(0x00)]
            public int Property1 { get; set; }

            [Offset(10, MinVersion = 10, MaxVersion = 20)]
            [Offset(15, MinVersion = 20)]
            public int Property2 { get; set; }

            [Offset(20, MinVersion = 10)]
            public int Property3 { get; set; }
        }
    }
}
