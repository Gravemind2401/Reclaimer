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
}
