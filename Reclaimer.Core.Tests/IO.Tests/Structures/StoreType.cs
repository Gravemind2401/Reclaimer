namespace Reclaimer.IO.Tests.Structures
{
    public class StoreTypeClass01
    {
        [Offset(0x00)]
        [StoreType(typeof(short))]
        public int Property1 { get; set; }

        [Offset(0x02)]
        [StoreType(typeof(byte))]
        public int Property2 { get; set; }

        [Offset(0x03)]
        [StoreType(typeof(float))]
        public double Property3 { get; set; }
    }

    [StructureDefinition<StoreTypeClass01_Builder, DefinitionBuilder>]
    public class StoreTypeClass01_Builder : StoreTypeClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<StoreTypeClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00).StoreType(typeof(short));
                v.Property(x => x.Property2).HasOffset(0x02).StoreType(typeof(byte));
                v.Property(x => x.Property3).HasOffset(0x03).StoreType(typeof(float));
            }
        }
    }
}
