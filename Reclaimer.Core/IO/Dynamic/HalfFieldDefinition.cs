namespace Reclaimer.IO.Dynamic
{
    internal class HalfFieldDefinition<TClass> : FieldDefinition<TClass, Half>
    {
        protected override Half StreamRead(EndianReader reader, ByteOrder byteOrder) => reader.ReadHalf(byteOrder);
        protected override void StreamWrite(EndianWriter writer, Half value, ByteOrder byteOrder) => writer.Write(value, byteOrder);
    }
}
