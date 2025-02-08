namespace Reclaimer.IO.Tests
{
    [TestClass]
    public class TestTransactionStream
    {
        [TestMethod]
        public void ContainedRead()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            var data = reader.ReadBytes(original.Length);
            Assert.AreEqual(original.Length, tran.Position);

            for (byte i = 0; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 5;
            writer.Write([9, 8, 7, 6, 5]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(10L, tran.Position);

            //contains a single patch in the middle
            tran.Position = 0;
            data = reader.ReadBytes(original.Length);
            Assert.AreEqual(original.Length, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 5; i < 10; i++)
                Assert.AreEqual((byte)(10 - (i - 4)), data[i]);
            for (byte i = 10; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 20;
            writer.Write([24, 23, 22, 21, 20]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(25L, tran.Position);

            //contains multiple patches in the middle
            tran.Position = 0;
            data = reader.ReadBytes(original.Length);
            Assert.AreEqual(original.Length, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 5; i < 10; i++)
                Assert.AreEqual((byte)(10 - (i - 4)), data[i]);
            for (byte i = 10; i < 20; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 20; i < 25; i++)
                Assert.AreEqual((byte)(25 - (i - 19)), data[i]);
            for (byte i = 25; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);
        }

        [TestMethod]
        public void AlignedRead()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            var data = reader.ReadBytes(original.Length);
            for (byte i = 0; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 5;
            writer.Write([9, 8, 7, 6, 5]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(10L, tran.Position);

            //aligned with a patch on both sides (same patch)
            tran.Position = 5;
            data = reader.ReadBytes(5);
            Assert.AreEqual(5 + 5L, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual((byte)(10 - (i + 1)), data[i]);

            //aligned on the left
            tran.Position = 5;
            data = reader.ReadBytes(10);
            Assert.AreEqual(5 + 10L, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual((byte)(10 - (i + 1)), data[i]);
            for (byte i = 5; i < 10; i++)
                Assert.AreEqual((byte)(i + 5), data[i]);

            //aligned on the right
            tran.Position = 0;
            data = reader.ReadBytes(10);
            Assert.AreEqual(10L, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 5; i < 10; i++)
                Assert.AreEqual((byte)(10 - (i - 4)), data[i]);

            tran.Position = 20;
            writer.Write([24, 23, 22, 21, 20]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(25L, tran.Position);

            //aligned with a different patch on each side
            tran.Position = 5;
            data = reader.ReadBytes(20);
            Assert.AreEqual(5 + 20L, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual((byte)(10 - (i + 1)), data[i]);
            for (byte i = 5; i < 15; i++)
                Assert.AreEqual((byte)(i + 5), data[i]);
            for (byte i = 15; i < 20; i++)
                Assert.AreEqual((byte)(25 - (i - 14)), data[i]);
        }

        [TestMethod]
        public void NonAlignedRead()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            var data = reader.ReadBytes(original.Length);
            for (byte i = 0; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 5;
            writer.Write([9, 8, 7, 6, 5]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(10L, tran.Position);

            //starts and ends within the same patch
            tran.Position = 6;
            data = reader.ReadBytes(3);
            Assert.AreEqual(6 + 3L, tran.Position);

            for (byte i = 0; i < 3; i++)
                Assert.AreEqual((byte)(8 - i), data[i]);

            //starts inside a patch
            tran.Position = 7;
            data = reader.ReadBytes(8);
            Assert.AreEqual(7 + 8L, tran.Position);

            for (byte i = 0; i < 3; i++)
                Assert.AreEqual((byte)(10 - (i + 3)), data[i]);
            for (byte i = 3; i < 8; i++)
                Assert.AreEqual((byte)(i + 7), data[i]);

            //ends inside a patch
            tran.Position = 0;
            data = reader.ReadBytes(8);
            Assert.AreEqual(8L, tran.Position);

            for (byte i = 0; i < 5; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 5; i < 8; i++)
                Assert.AreEqual((byte)(10 - (i - 4)), data[i]);

            tran.Position = 20;
            writer.Write([24, 23, 22, 21, 20]);

            Assert.AreEqual(255L, tran.Length);
            Assert.AreEqual(25L, tran.Position);

            //starts in one patch and ends in another
            tran.Position = 7;
            data = reader.ReadBytes(16);
            Assert.AreEqual(7 + 16L, tran.Position);

            for (byte i = 0; i < 3; i++)
                Assert.AreEqual((byte)(10 - (i + 3)), data[i]);
            for (byte i = 3; i < 13; i++)
                Assert.AreEqual((byte)(i + 7), data[i]);
            for (byte i = 13; i < 16; i++)
                Assert.AreEqual((byte)(25 - (i - 12)), data[i]);
        }

        [TestMethod]
        public void ContainedWrite()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            tran.Position = 40;
            writer.Write(Enumerable.Repeat((byte)99, 20).ToArray());
            Assert.AreEqual(40 + 20L, tran.Position);

            tran.Position = 0;
            var data = reader.ReadBytes(original.Length);
            for (byte i = 0; i < 40; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 40; i < 60; i++)
                Assert.AreEqual((byte)99, data[i]);
            for (byte i = 60; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 30;
            writer.Write(Enumerable.Repeat((byte)77, 40).ToArray());
            Assert.AreEqual(30 + 40L, tran.Position);

            tran.Position = 0;
            data = reader.ReadBytes(original.Length);
            for (byte i = 0; i < 30; i++)
                Assert.AreEqual(i, data[i]);
            for (byte i = 30; i < 70; i++)
                Assert.AreEqual((byte)77, data[i]);
            for (byte i = 70; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 0;
            writer.Write(new byte[original.Length]);
            Assert.AreEqual(original.Length, tran.Position);

            tran.Position = 0;
            data = reader.ReadBytes(original.Length);
            for (var i = byte.MinValue; i < byte.MaxValue; i++)
                Assert.AreEqual(byte.MinValue, data[i]);

            Assert.AreEqual(255L, tran.Length);
        }

        [TestMethod]
        public void AlignedWrite()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            //first patch
            tran.Position = 30;
            writer.Write(Enumerable.Repeat((byte)55, 10).ToArray());
            Assert.AreEqual(30 + 10L, tran.Position);

            tran.Position = 30;
            var data = reader.ReadBytes(10);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)55, data[i]);

            //second patch
            tran.Position = 50;
            writer.Write(Enumerable.Repeat((byte)99, 10).ToArray());
            Assert.AreEqual(50 + 10L, tran.Position);

            tran.Position = 50;
            data = reader.ReadBytes(10);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)99, data[i]);

            //write over the same bytes as the first patch
            tran.Position = 30;
            writer.Write(Enumerable.Repeat((byte)5, 10).ToArray());

            tran.Position = 30;
            data = reader.ReadBytes(10);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)5, data[i]);

            //write from the start of the first patch to the end of the second patch
            tran.Position = 30;
            writer.Write(Enumerable.Repeat((byte)42, 30).ToArray());

            tran.Position = 30;
            data = reader.ReadBytes(30);
            for (byte i = 0; i < 30; i++)
                Assert.AreEqual((byte)42, data[i]);

            Assert.AreEqual(255L, tran.Length);
        }

        [TestMethod]
        public void NonAlignedWrite()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            //separate patch
            tran.Position = 10;
            writer.Write(Enumerable.Repeat((byte)8, 10).ToArray());
            Assert.AreEqual(10 + 10L, tran.Position);

            tran.Position = 10;
            var data = reader.ReadBytes(10);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)8, data[i]);

            //write completely within the patch
            tran.Position = 12;
            writer.Write(Enumerable.Repeat((byte)0, 6).ToArray());
            Assert.AreEqual(12 + 6L, tran.Position);

            tran.Position = 10;
            data = reader.ReadBytes(10);
            for (byte i = 0; i < 2; i++)
                Assert.AreEqual((byte)8, data[i]);
            for (byte i = 2; i < 8; i++)
                Assert.AreEqual((byte)0, data[i]);
            for (byte i = 8; i < 10; i++)
                Assert.AreEqual((byte)8, data[i]);

            //first patch
            tran.Position = 40;
            writer.Write(Enumerable.Repeat((byte)1, 10).ToArray());
            Assert.AreEqual(40 + 10L, tran.Position);

            tran.Position = 40;
            data = reader.ReadBytes(10);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)1, data[i]);

            //second patch
            tran.Position = 50;
            writer.Write(Enumerable.Repeat((byte)2, 10).ToArray());
            Assert.AreEqual(50 + 10L, tran.Position);

            tran.Position = 40;
            data = reader.ReadBytes(20);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)1, data[i]);
            for (byte i = 10; i < 20; i++)
                Assert.AreEqual((byte)2, data[i]);

            //write into the first patch
            tran.Position = 35;
            writer.Write(Enumerable.Repeat((byte)3, 10).ToArray());
            Assert.AreEqual(35 + 10L, tran.Position);

            tran.Position = 35;
            data = reader.ReadBytes(25);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)3, data[i]);
            for (byte i = 10; i < 15; i++)
                Assert.AreEqual((byte)1, data[i]);
            for (byte i = 15; i < 25; i++)
                Assert.AreEqual((byte)2, data[i]);

            //write out of the second patch
            tran.Position = 55;
            writer.Write(Enumerable.Repeat((byte)4, 10).ToArray());
            Assert.AreEqual(55 + 10L, tran.Position);

            tran.Position = 35;
            data = reader.ReadBytes(30);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)3, data[i]);
            for (byte i = 10; i < 15; i++)
                Assert.AreEqual((byte)1, data[i]);
            for (byte i = 15; i < 20; i++)
                Assert.AreEqual((byte)2, data[i]);
            for (byte i = 20; i < 30; i++)
                Assert.AreEqual((byte)4, data[i]);

            //write over the end of one patch and start of another
            tran.Position = 45;
            writer.Write(Enumerable.Repeat((byte)99, 10).ToArray());
            Assert.AreEqual(45 + 10L, tran.Position);

            tran.Position = 35;
            data = reader.ReadBytes(30);
            for (byte i = 0; i < 10; i++)
                Assert.AreEqual((byte)3, data[i]);
            for (byte i = 10; i < 20; i++)
                Assert.AreEqual((byte)99, data[i]);
            for (byte i = 20; i < 30; i++)
                Assert.AreEqual((byte)4, data[i]);

            Assert.AreEqual(255L, tran.Length);
        }

        [TestMethod]
        public void ExpandingWrite()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            //start within the source stream, then write past the end of it
            tran.Position = 240;
            writer.Write(Enumerable.Repeat((byte)99, 30).ToArray());
            Assert.AreEqual(270L, tran.Position);
            Assert.AreEqual(270L, tran.Length);

            //seek past the end of the source stream, then start writing
            tran.Position = 300;
            writer.Write(Enumerable.Repeat((byte)77, 30).ToArray());
            Assert.AreEqual(330L, tran.Position);
            Assert.AreEqual(330L, tran.Length);

            //read beyond the source stream but between patches
            tran.Position = 270;
            var data = reader.ReadBytes(30);
            Assert.AreEqual(300L, tran.Position);

            for (var i = 0; i < 30; i++)
                Assert.AreEqual(byte.MinValue, data[i]);

            //read beyond the source stream including patches
            tran.Position = 0;
            data = reader.ReadBytes(330);
            Assert.AreEqual(330L, tran.Position);

            for (byte i = 0; i < 240; i++)
                Assert.AreEqual(i, data[i]);
            for (var i = 240; i < 270; i++)
                Assert.AreEqual((byte)99, data[i]);
            for (var i = 270; i < 300; i++)
                Assert.AreEqual(byte.MinValue, data[i]);
            for (var i = 300; i < 330; i++)
                Assert.AreEqual((byte)77, data[i]);
        }

        [TestMethod]
        public void ExpandingWrite2()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);
            var writer = new EndianWriter(tran);

            // try to read past the end without expanding
            tran.Position = 200;
            var data = reader.ReadBytes(100);
            Assert.AreEqual(55, data.Length);
            Assert.AreEqual(255L, tran.Position);

            for (var i = 0; i < 55; i++)
                Assert.AreEqual((byte)(200 + i), data[i]);

            //seek past the end of the source stream, then start writing
            tran.Position = 300;
            writer.Write(Enumerable.Repeat((byte)99, 50).ToArray());
            Assert.AreEqual(350L, tran.Position);
            Assert.AreEqual(350L, tran.Length);

            // try to read past the end again
            tran.Position = 200;
            data = reader.ReadBytes(100);
            Assert.AreEqual(100, data.Length);
            Assert.AreEqual(300L, tran.Position);

            for (var i = 0; i < 55; i++)
                Assert.AreEqual((byte)(200 + i), data[i]);
            for (var i = 55; i < 100; i++)
                Assert.AreEqual(byte.MinValue, data[i]);

            // start in virtual space, then try to read past the virtual end
            tran.Position = 275;
            data = reader.ReadBytes(100);
            Assert.AreEqual(75, data.Length);
            Assert.AreEqual(350L, tran.Position);

            for (var i = 0; i < 25; i++)
                Assert.AreEqual(byte.MinValue, data[i]);
            for (var i = 25; i < 75; i++)
                Assert.AreEqual((byte)99, data[i]);
        }

        [TestMethod]
        public void Shrinking()
        {
            var original = new byte[byte.MaxValue];
            for (byte i = 0; i < original.Length; i++)
                original[i] = i;

            var ms = new MemoryStream(original);
            var tran = new TransactionStream(ms);

            var reader = new EndianReader(tran);

            Assert.AreEqual(byte.MaxValue, tran.Length);

            tran.Position = 0;
            var data = reader.ReadBytes(original.Length);
            Assert.AreEqual(byte.MaxValue, data.Length);

            for (byte i = 0; i < original.Length; i++)
                Assert.AreEqual(i, data[i]);

            tran.SetLength(100);
            Assert.AreEqual(100L, tran.Length);

            tran.Position = 0;
            data = reader.ReadBytes(original.Length);
            Assert.AreEqual(100, data.Length);

            for (byte i = 0; i < 100; i++)
                Assert.AreEqual(i, data[i]);

            tran.Position = 200;
            data = reader.ReadBytes(50);
            Assert.AreEqual(0, data.Length);
            Assert.AreEqual(100L, tran.Length);
        }
    }
}
