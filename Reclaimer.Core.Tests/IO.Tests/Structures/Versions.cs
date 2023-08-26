namespace Reclaimer.IO.Tests.Structures
{
    public class VersionedClass01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        [VersionNumber]
        public int Version { get; set; }

        [Offset(0x08, MaxVersion = 2)]
        [Offset(0x0C, MinVersion = 2)]
        public float Property2 { get; set; }

        [Offset(0x10)]
        [MinVersion(2)]
        [MaxVersion(4)]
        public float? Property3 { get; set; }

        [Offset(0x14)]
        [VersionSpecific(4)]
        public double? Property4 { get; set; }

        [Offset(0x1C)]
        [MinVersion(4)]
        [MaxVersion(4)]
        public double? Property5 { get; set; }
    }

    public class VersionedClass02a
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        public int Version { get; set; }

        [Offset(0x08, MaxVersion = 2)]
        [Offset(0x0C, MinVersion = 2)]
        public float Property2 { get; set; }

        [Offset(0x10)]
        [MinVersion(2)]
        [MaxVersion(4)]
        public float? Property3 { get; set; }

        [Offset(0x14)]
        [VersionSpecific(4)]
        public double? Property4 { get; set; }
    }

    public class VersionedClass02b
    {
        [Offset(0x00)]
        public int Property1 { get; set; }

        [Offset(0x04)]
        [VersionNumber]
        public int Version { get; set; }

        [Offset(0x08, MaxVersion = 2)]
        [Offset(0x0C, MinVersion = 2)]
        public float Property2 { get; set; }

        [Offset(0x10)]
        [MinVersion(2)]
        [MaxVersion(4)]
        public float? Property3 { get; set; }

        [Offset(0x14)]
        [VersionSpecific(4)]
        public double? Property4 { get; set; }
    }

    [FixedSize(0x20, MaxVersion = 3)]
    [FixedSize(0x30, MinVersion = 3, MaxVersion = 5)]
    [FixedSize(0x40, MinVersion = 5)]
    public class VersionedClass03
    {
        [Offset(0x08, MaxVersion = 2)]
        [Offset(0x18, MinVersion = 2, MaxVersion = 4)]
        [Offset(0x28, MinVersion = 4)]
        public int Property1 { get; set; }
    }

    [FixedSize(0x20, MaxVersion = 3)]
    [FixedSize(0x30, MinVersion = 3, MaxVersion = 5)]
    [FixedSize(0x40, MinVersion = 5)]
    public class VersionedClass04
    {
        [Offset(0x10)]
        public int Property1 { get; set; }
    }

    public class VersionedClass05
    {
        [Offset(0x10)]
        [MaxVersion(3)]
        public int? Property1a { get; set; }

        [Offset(0x10)]
        [MinVersion(3)]
        public int? Property1b { get; set; }
    }

    [StructureDefinition<VersionedClass01_Builder, DefinitionBuilder>]
    public class VersionedClass01_Builder : VersionedClass01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(null, 2);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(2, 4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property3).HasOffset(0x10);

                v = AddVersion<int>(4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property4).HasOffset(0x14);
                v.Property(x => x.Property5).HasOffset(0x1C);
            }
        }
    }

    [StructureDefinition<VersionedClass02a_Builder, DefinitionBuilder>]
    public class VersionedClass02a_Builder : VersionedClass02a
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass02a_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04);
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(null, 2);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04);
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(2, 4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04);
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property3).HasOffset(0x10);

                v = AddVersion<int>(4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04);
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property4).HasOffset(0x14);
            }
        }
    }

    [StructureDefinition<VersionedClass02b_Builder, DefinitionBuilder>]
    public class VersionedClass02b_Builder : VersionedClass02b
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass02b_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(null, 2);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x08);

                v = AddVersion<int>(2, 4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property3).HasOffset(0x10);

                v = AddVersion<int>(4);

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Version).HasOffset(0x04).IsVersionNumber();
                v.Property(x => x.Property2).HasOffset(0x0C);
                v.Property(x => x.Property4).HasOffset(0x14);
            }
        }
    }

    [StructureDefinition<VersionedClass03_Builder, DefinitionBuilder>]
    public class VersionedClass03_Builder : VersionedClass03
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass03_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddVersion<int>(null, 2).HasFixedSize(0x20);
                v.Property(x => x.Property1).HasOffset(0x08);

                v = AddVersion<int>(2, 3).HasFixedSize(0x20);
                v.Property(x => x.Property1).HasOffset(0x18);

                v = AddVersion<int>(3, 4).HasFixedSize(0x30);
                v.Property(x => x.Property1).HasOffset(0x18);

                v = AddVersion<int>(4, 5).HasFixedSize(0x30);
                v.Property(x => x.Property1).HasOffset(0x28);

                v = AddVersion<int>(5, null).HasFixedSize(0x40);
                v.Property(x => x.Property1).HasOffset(0x28);
            }
        }
    }

    [StructureDefinition<VersionedClass04_Builder, DefinitionBuilder>]
    public class VersionedClass04_Builder : VersionedClass04
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass04_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddVersion<int>(null, 2).HasFixedSize(0x20);
                v.Property(x => x.Property1).HasOffset(0x10);

                v = AddVersion<int>(2, 3).HasFixedSize(0x20);
                v.Property(x => x.Property1).HasOffset(0x10);

                v = AddVersion<int>(3, 4).HasFixedSize(0x30);
                v.Property(x => x.Property1).HasOffset(0x10);

                v = AddVersion<int>(4, 5).HasFixedSize(0x30);
                v.Property(x => x.Property1).HasOffset(0x10);

                v = AddVersion<int>(5, null).HasFixedSize(0x40);
                v.Property(x => x.Property1).HasOffset(0x10);
            }
        }
    }

    [StructureDefinition<VersionedClass05_Builder, DefinitionBuilder>]
    public class VersionedClass05_Builder : VersionedClass05
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<VersionedClass05_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddVersion<int>(null, 3);
                v.Property(x => x.Property1a).HasOffset(0x10);

                v = AddVersion<int>(3, null);
                v.Property(x => x.Property1b).HasOffset(0x10);
            }
        }
    }
}
