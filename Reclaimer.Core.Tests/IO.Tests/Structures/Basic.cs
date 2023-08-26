namespace Reclaimer.IO.Tests.Structures
{
    [FixedSize(0xFF)]
    public class BasicClass01
    {
        [Offset(0x00)]
        public sbyte Property1 { get; set; }

        [Offset(0x10)]
        public short Property2 { get; set; }

        [Offset(0x20)]
        public int Property3 { get; set; }

        [Offset(0x30)]
        public long Property4 { get; set; }

        [Offset(0x40)]
        public byte Property5 { get; set; }

        [Offset(0x50)]
        public ushort Property6 { get; set; }

        [Offset(0x60)]
        public uint Property7 { get; set; }

        [Offset(0x70)]
        public ulong Property8 { get; set; }

        [Offset(0x80)]
        public float Property9 { get; set; }

        [Offset(0x90)]
        public double Property10 { get; set; }

        [Offset(0xA0)]
        public Guid Property11 { get; set; }
    }

    public class BasicClass02
    {
        [Offset(0x70)]
        public sbyte Property1 { get; set; }

        [Offset(0x40)]
        public short Property2 { get; set; }

        [Offset(0x30)]
        public int Property3 { get; set; }

        [Offset(0x10)]
        public long Property4 { get; set; }

        [Offset(0x90)]
        public byte Property5 { get; set; }

        [Offset(0xA0)]
        public ushort Property6 { get; set; }

        [Offset(0x00)]
        public uint Property7 { get; set; }

        [Offset(0x80)]
        public ulong Property8 { get; set; }

        [Offset(0x20)]
        public float Property9 { get; set; }

        [Offset(0x50)]
        public double Property10 { get; set; }

        [Offset(0x60)]
        public Guid Property11 { get; set; }
    }

    [FixedSize(0xFF)]
    public class FactoryClass01
    {
        //no public or parameterless constructors
        private FactoryClass01(int param)
        {

        }

        public static FactoryClass01 GetInstance()
        {
            return new FactoryClass01(0);
        }

        [Offset(0x00)]
        public sbyte Property1 { get; set; }

        [Offset(0x10)]
        public short Property2 { get; set; }

        [Offset(0x20)]
        public int Property3 { get; set; }

        [Offset(0x30)]
        public long Property4 { get; set; }

        [Offset(0x40)]
        public byte Property5 { get; set; }

        [Offset(0x50)]
        public ushort Property6 { get; set; }

        [Offset(0x60)]
        public uint Property7 { get; set; }

        [Offset(0x70)]
        public ulong Property8 { get; set; }

        [Offset(0x80)]
        public float Property9 { get; set; }

        [Offset(0x90)]
        public double Property10 { get; set; }

        [Offset(0xA0)]
        public Guid Property11 { get; set; }
    }
}
