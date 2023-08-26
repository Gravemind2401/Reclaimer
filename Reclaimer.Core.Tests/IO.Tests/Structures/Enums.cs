namespace Reclaimer.IO.Tests.Structures
{
    public class EnumClass01
    {
        [Offset(0x00)]
        public Enum8 Property1 { get; set; }

        [Offset(0x01)]
        public Enum16 Property2 { get; set; }

        [Offset(0x03)]
        public Enum32 Property3 { get; set; }

        [Offset(0x07)]
        public Enum64 Property4 { get; set; }
    }

    public enum Enum8 : byte
    {
        Value01 = 1,
        Value02 = 2,
        Value03 = 3,
    }

    public enum Enum16 : short
    {
        Value01 = 4,
        Value02 = 5,
        Value03 = 6,
    }

    public enum Enum32
    {
        Value01 = 7,
        Value02 = 8,
        Value03 = 9,
    }

    public enum Enum64 : long
    {
        Value01 = 10,
        Value02 = 11,
        Value03 = 12,
    }
}
