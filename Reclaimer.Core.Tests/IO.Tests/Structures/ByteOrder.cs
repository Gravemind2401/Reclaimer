namespace Reclaimer.IO.Tests.Structures
{
    [FixedSize(0xFF)]
    [ByteOrder(ByteOrder.BigEndian)]
    public class ByteOrderClass01
    {
        [Offset(0x00)]
        public sbyte Property1 { get; set; }

        [Offset(0x10)]
        public short Property2 { get; set; }

        [Offset(0x20)]
        public int Property3 { get; set; }

        [Offset(0x30)]
        [ByteOrder(ByteOrder.LittleEndian)]
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
        public Half Property9 { get; set; }

        [Offset(0x90)]
        public float Property10 { get; set; }

        [Offset(0xA0)]
        public double Property11 { get; set; }

        [Offset(0xB0)]
        public Guid Property12 { get; set; }
    }

    public class ByteOrderClass02
    {
        [Offset(0x70)]
        public sbyte Property1 { get; set; }

        [Offset(0x40)]
        public short Property2 { get; set; }

        [Offset(0x30)]
        public int Property3 { get; set; }

        [Offset(0x10)]
        [ByteOrder(ByteOrder.LittleEndian)]
        public long Property4 { get; set; }

        [Offset(0x90)]
        public byte Property5 { get; set; }

        [Offset(0xA0)]
        public ushort Property6 { get; set; }

        [Offset(0x00)]
        public uint Property7 { get; set; }

        [Offset(0x80)]
        [ByteOrder(ByteOrder.BigEndian)]
        public ulong Property8 { get; set; }

        [Offset(0xB0)]
        public Half Property9 { get; set; }

        [Offset(0x20)]
        public float Property10 { get; set; }

        [Offset(0x50)]
        public double Property11 { get; set; }

        [Offset(0x60)]
        public Guid Property12 { get; set; }
    }
}
