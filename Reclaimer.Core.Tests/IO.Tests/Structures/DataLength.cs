namespace Reclaimer.IO.Tests.Structures
{
    public class DataLengthClass01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [DataLength]
        [Offset(0x04)]
        public int Property2 { get; set; }
    }

    [StructureDefinition<DataLengthClass01_Builder, DefinitionBuilder>]
    public class DataLengthClass01_Builder : DataLengthClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<DataLengthClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x04).IsDataLength();
            }
        }
    }
}
