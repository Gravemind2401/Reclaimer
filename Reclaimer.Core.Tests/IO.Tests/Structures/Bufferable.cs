namespace Reclaimer.IO.Tests.Structures
{
    public struct BufferableStruct01 : IBufferable<BufferableStruct01>
    {
        public static int PackSize => sizeof(int);
        public static int SizeOf => sizeof(int) + sizeof(uint) + sizeof(float);

        public int Property1 { get; set; }
        public uint Property2 { get; set; }
        public float Property3 { get; set; }

        public static BufferableStruct01 ReadFromBuffer(ReadOnlySpan<byte> buffer)
        {
            return new BufferableStruct01
            {
                Property1 = BitConverter.ToInt32(buffer),
                Property2 = BitConverter.ToUInt32(buffer[4..]),
                Property3 = BitConverter.ToSingle(buffer[8..]),
            };
        }

        public readonly void WriteToBuffer(Span<byte> buffer)
        {
            BitConverter.GetBytes(Property1).CopyTo(buffer);
            BitConverter.GetBytes(Property2).CopyTo(buffer[4..]);
            BitConverter.GetBytes(Property3).CopyTo(buffer[8..]);
        }
    }

    public class ClassWithBufferable01
    {
        [Offset(0x00)]
        public int Property1 { get; set; }
        [Offset(0x10)]
        public uint Property2 { get; set; }
        [Offset(0x20)]
        public BufferableStruct01 Property3 { get; set; }
    }

    [StructureDefinition<ClassWithBufferable01_Builder, DefinitionBuilder>]
    public class ClassWithBufferable01_Builder : ClassWithBufferable01
    {
        private class DefinitionBuilder : Dynamic.DefinitionBuilder<ClassWithBufferable01_Builder>
        {
            public DefinitionBuilder()
            {
                var v = AddDefaultVersion();

                v.Property(x => x.Property1).HasOffset(0x00);
                v.Property(x => x.Property2).HasOffset(0x10);
                v.Property(x => x.Property3).HasOffset(0x20);
            }
        }
    }
}
