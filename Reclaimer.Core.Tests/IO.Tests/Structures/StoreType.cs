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
}
