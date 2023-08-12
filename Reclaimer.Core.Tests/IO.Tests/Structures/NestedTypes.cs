namespace Reclaimer.IO.Tests.Structures
{
    public class OuterClass01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public InnerClass01 Property2 { get; set; }

        [Offset(0x10)]
        public int Property3 { get; set; }

        [Offset(0x14)]
        public InnerStruct01 Property4 { get; set; }

        [Offset(0x20)]
        public int Property5 { get; set; }
    }

    public struct OuterStruct01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public InnerClass01 Property2 { get; set; }

        [Offset(0x10)]
        public int Property3 { get; set; }

        [Offset(0x14)]
        public InnerStruct01 Property4 { get; set; }

        [Offset(0x20)]
        public int Property5 { get; set; }
    }

    public class InnerClass01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public int Property2 { get; set; }

        [Offset(0x08)]
        public int Property3 { get; set; }
    }

    public struct InnerStruct01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public int Property2 { get; set; }

        [Offset(0x08)]
        public int Property3 { get; set; }
    }
}
