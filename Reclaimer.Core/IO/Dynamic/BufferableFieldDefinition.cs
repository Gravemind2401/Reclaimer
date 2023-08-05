namespace Reclaimer.IO.Dynamic
{
    internal class BufferableFieldDefinition<TClass, TBufferable> : FieldDefinition<TClass, TBufferable>
        where TBufferable : IBufferable<TBufferable>
    {
        protected override TBufferable StreamRead(EndianReader reader, ByteOrder byteOrder) => reader.ReadBufferable<TBufferable>(byteOrder);
        protected override void StreamWrite(EndianWriter writer, TBufferable value, ByteOrder byteOrder) => writer.WriteBufferable(value, byteOrder);
    }
}
