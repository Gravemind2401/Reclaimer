namespace Reclaimer.IO.Tests.Structures
{
    public interface IOuterType<TInnerClass, TInnerStruct>
        where TInnerClass : class, IInnerType
        where TInnerStruct : struct, IInnerType
    {
        int Property1 { get; set; }
        TInnerClass Property2 { get; set; }
        int Property3 { get; set; }
        TInnerStruct Property4 { get; set; }
        int Property5 { get; set; }
    }

    public interface IInnerType
    {
        int Property1 { get; set; }
        int Property2 { get; set; }
        int Property3 { get; set; }
    }

    public class OuterClass01 : IOuterType<InnerClass01, InnerStruct01>
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

    public struct OuterStruct01 : IOuterType<InnerClass01, InnerStruct01>
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

    public class InnerClass01 : IInnerType
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public int Property2 { get; set; }

        [Offset(0x08)]
        public int Property3 { get; set; }
    }

    public struct InnerStruct01 : IInnerType
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public int Property2 { get; set; }

        [Offset(0x08)]
        public int Property3 { get; set; }
    }

    [StructureDefinition<OuterClass01_Builder, DefinitionBuilder>]
    public class OuterClass01_Builder : IOuterType<InnerClass01_Builder, InnerStruct01_Builder>
    {
        public int Property1 { get; set; }
        public InnerClass01_Builder Property2 { get; set; }
        public int Property3 { get; set; }
        public InnerStruct01_Builder Property4 { get; set; }
        public int Property5 { get; set; }

        private class DefinitionBuilder : Dynamic.DefinitionBuilder<OuterClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x04);
                v.Property(x => x.Property3).HasOffset(0x10);
                v.Property(x => x.Property4).HasOffset(0x14);
                v.Property(x => x.Property5).HasOffset(0x20);
            }
        }
    }

    [StructureDefinition<OuterStruct01_Builder, DefinitionBuilder>]
    public struct OuterStruct01_Builder : IOuterType<InnerClass01_Builder, InnerStruct01_Builder>
    {
        public int Property1 { get; set; }
        public InnerClass01_Builder Property2 { get; set; }
        public int Property3 { get; set; }
        public InnerStruct01_Builder Property4 { get; set; }
        public int Property5 { get; set; }

        private class DefinitionBuilder : Dynamic.DefinitionBuilder<OuterStruct01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x04);
                v.Property(x => x.Property3).HasOffset(0x10);
                v.Property(x => x.Property4).HasOffset(0x14);
                v.Property(x => x.Property5).HasOffset(0x20);
            }
        }
    }

    [StructureDefinition<InnerClass01_Builder, DefinitionBuilder>]
    public class InnerClass01_Builder : InnerClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<InnerClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x04);
                v.Property(x => x.Property3).HasOffset(0x08);
            }
        }
    }

    [StructureDefinition<InnerStruct01_Builder, DefinitionBuilder>]
    public struct InnerStruct01_Builder : IInnerType
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public int Property3 { get; set; }

        private class DefinitionBuilder : Dynamic.DefinitionBuilder<InnerStruct01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x04);
                v.Property(x => x.Property3).HasOffset(0x08);
            }
        }
    }
}
