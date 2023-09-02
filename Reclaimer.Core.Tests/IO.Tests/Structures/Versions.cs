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
}
