namespace Reclaimer.IO.Tests.Structures
{
    public class DerivedReader : EndianReader
    {
        public DerivedReader(Stream input, ByteOrder byteOrder)
            : base(input, byteOrder)
        { }

        protected override T CreateInstance<T>(double? version)
        {
            if (typeof(T) == typeof(DerivedTestClass01))
                return (T)(object)new DerivedTestClass01(this);

            if (typeof(T) == typeof(DerivedTestClass02))
                return (T)(object)new DerivedTestClass02(this);

            if (typeof(T) == typeof(DerivedTestClass03))
                return (T)(object)new DerivedTestClass03(this);

            return base.CreateInstance<T>(version);
        }
    }

    public class DerivedTestClass01
    {
        public int Property0 { get; }

        public DerivedTestClass01(EndianReader reader)
        {
            Property0 = reader.ReadInt32();
        }
    }

    public class DerivedTestClass02
    {
        public int Property0 { get; }

        [Offset(0x04)]
        public int Property1 { get; set; }

        public DerivedTestClass02(EndianReader reader)
        {
            Property0 = reader.ReadInt32();
        }
    }

    [FixedSize(0x10)]
    public class DerivedTestClass03
    {
        public int Property0 { get; }

        public DerivedTestClass03(EndianReader reader)
        {
            Property0 = reader.ReadInt32();
        }
    }
}
