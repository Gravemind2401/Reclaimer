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
}
