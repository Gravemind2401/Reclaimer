using System.Linq.Expressions;

namespace Reclaimer.IO.Dynamic
{
    internal class PrimitiveFieldDefinition<TClass, TField> : FieldDefinition<TClass, TField>
        where TField : struct
    {
        private delegate TField ReadMethod(EndianReader reader, ByteOrder byteOrder);
        private delegate void WriteMethod(EndianWriter writer, TField value, ByteOrder byteOrder);

        private static readonly ReadMethod InvokeRead;
        private static readonly WriteMethod InvokeWrite;

        static PrimitiveFieldDefinition()
        {
            var typeCode = Type.GetTypeCode(typeof(TField));

            InvokeRead = typeCode switch
            {
                TypeCode.Int16 => CreateReadDelegate((r, o) => r.ReadInt16(o)),
                TypeCode.UInt16 => CreateReadDelegate((r, o) => r.ReadUInt16(o)),
                TypeCode.Int32 => CreateReadDelegate((r, o) => r.ReadInt32(o)),
                TypeCode.UInt32 => CreateReadDelegate((r, o) => r.ReadUInt32(o)),
                TypeCode.Int64 => CreateReadDelegate((r, o) => r.ReadInt64(o)),
                TypeCode.UInt64 => CreateReadDelegate((r, o) => r.ReadUInt64(o)),
                TypeCode.Single => CreateReadDelegate((r, o) => r.ReadSingle(o)),
                TypeCode.Double => CreateReadDelegate((r, o) => r.ReadDouble(o)),
                _ when typeof(TField) == typeof(Guid) => CreateReadDelegate((r, o) => r.ReadGuid(o)),
                _ when typeof(TField) == typeof(decimal) => CreateReadDelegate((r, o) => r.ReadDecimal(o)),
                _ => throw new NotSupportedException()
            };

            InvokeWrite = typeCode switch
            {
                TypeCode.Int16 => CreateWriteDelegate<short>((w, o, v) => w.Write(v, o)),
                TypeCode.UInt16 => CreateWriteDelegate<ushort>((w, o, v) => w.Write(v, o)),
                TypeCode.Int32 => CreateWriteDelegate<int>((w, o, v) => w.Write(v, o)),
                TypeCode.UInt32 => CreateWriteDelegate<uint>((w, o, v) => w.Write(v, o)),
                TypeCode.Int64 => CreateWriteDelegate<long>((w, o, v) => w.Write(v, o)),
                TypeCode.UInt64 => CreateWriteDelegate<ulong>((w, o, v) => w.Write(v, o)),
                TypeCode.Single => CreateWriteDelegate<float>((w, o, v) => w.Write(v, o)),
                TypeCode.Double => CreateWriteDelegate<double>((w, o, v) => w.Write(v, o)),
                _ when typeof(TField) == typeof(Guid) => CreateWriteDelegate<Guid>((w, o, v) => w.Write(v, o)),
                _ when typeof(TField) == typeof(decimal) => CreateWriteDelegate<decimal>((w, o, v) => w.Write(v, o)),
                _ => throw new NotSupportedException()
            };

            static ReadMethod CreateReadDelegate<TRead>(Expression<Func<EndianReader, ByteOrder, TRead>> expression)
            {
                if (expression.Body is not MethodCallExpression methodCall)
                    throw new Exception();

                var method = methodCall.Method;
                if (method == null)
                    throw new Exception();

                if (typeof(TRead) != typeof(TField))
                    throw new Exception();

                var d = method.CreateDelegate<ReadMethod>();
                return d;
            }

            static WriteMethod CreateWriteDelegate<TWrite>(Expression<Action<EndianWriter, ByteOrder, TWrite>> expression)
            {
                if (expression.Body is not MethodCallExpression methodCall)
                    throw new Exception();

                var method = methodCall.Method;
                if (method == null)
                    throw new Exception();

                if (typeof(TWrite) != typeof(TField))
                    throw new Exception();

                var d = method.CreateDelegate<WriteMethod>();
                return d;
            }
        }

        protected override TField StreamRead(EndianReader reader, ByteOrder byteOrder) => InvokeRead(reader, byteOrder);
        protected override void StreamWrite(EndianWriter writer, TField value, ByteOrder byteOrder) => InvokeWrite(writer, value, byteOrder);
    }
}
