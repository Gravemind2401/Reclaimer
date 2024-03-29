﻿namespace Reclaimer.IO.Tests.Structures
{
    [FixedSize(0xFF)]
    public class NullablesClass01
    {
        [Offset(0x00)]
        public sbyte? Property1 { get; set; }

        [Offset(0x10)]
        public short? Property2 { get; set; }

        [Offset(0x20)]
        public int? Property3 { get; set; }

        [Offset(0x30)]
        public long? Property4 { get; set; }

        [Offset(0x40)]
        public byte? Property5 { get; set; }

        [Offset(0x50)]
        public ushort? Property6 { get; set; }

        [Offset(0x60)]
        public uint? Property7 { get; set; }

        [Offset(0x70)]
        public ulong? Property8 { get; set; }

        [Offset(0x80)]
        public Half? Property9 { get; set; }

        [Offset(0x90)]
        public float? Property10 { get; set; }

        [Offset(0xA0)]
        public double? Property11 { get; set; }

        [Offset(0xB0)]
        public Guid? Property12 { get; set; }
    }

    public class NullablesClass02
    {
        [Offset(0x70)]
        public sbyte? Property1 { get; set; }

        [Offset(0x40)]
        public short? Property2 { get; set; }

        [Offset(0x30)]
        public int? Property3 { get; set; }

        [Offset(0x10)]
        public long? Property4 { get; set; }

        [Offset(0x90)]
        public byte? Property5 { get; set; }

        [Offset(0xA0)]
        public ushort? Property6 { get; set; }

        [Offset(0x00)]
        public uint? Property7 { get; set; }

        [Offset(0x80)]
        public ulong? Property8 { get; set; }

        [Offset(0xB0)]
        public Half? Property9 { get; set; }

        [Offset(0x20)]
        public float? Property10 { get; set; }

        [Offset(0x50)]
        public double? Property11 { get; set; }

        [Offset(0x60)]
        public Guid? Property12 { get; set; }
    }

    [StructureDefinition<NullablesClass01_Builder, DefinitionBuilder>]
    public class NullablesClass01_Builder : NullablesClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<NullablesClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion().HasFixedSize(0xFF);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x10);
                v.Property(x => x.Property3).HasOffset(0x20);
                v.Property(x => x.Property4).HasOffset(0x30);
                v.Property(x => x.Property5).HasOffset(0x40);
                v.Property(x => x.Property6).HasOffset(0x50);
                v.Property(x => x.Property7).HasOffset(0x60);
                v.Property(x => x.Property8).HasOffset(0x70);
                v.Property(x => x.Property9).HasOffset(0x80);
                v.Property(x => x.Property10).HasOffset(0x90);
                v.Property(x => x.Property11).HasOffset(0xA0);
                v.Property(x => x.Property12).HasOffset(0xB0);
            }
        }
    }

    [StructureDefinition<NullablesClass02_Builder, DefinitionBuilder>]
    public class NullablesClass02_Builder : NullablesClass02
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<NullablesClass02_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x70);
                v.Property(x => x.Property2).HasOffset(0x40);
                v.Property(x => x.Property3).HasOffset(0x30);
                v.Property(x => x.Property4).HasOffset(0x10);
                v.Property(x => x.Property5).HasOffset(0x90);
                v.Property(x => x.Property6).HasOffset(0xA0);
                v.Property(x => x.Property7).HasOffset(0x00);
                v.Property(x => x.Property8).HasOffset(0x80);
                v.Property(x => x.Property9).HasOffset(0xB0);
                v.Property(x => x.Property10).HasOffset(0x20);
                v.Property(x => x.Property11).HasOffset(0x50);
                v.Property(x => x.Property12).HasOffset(0x60);
            }
        }
    }
}
