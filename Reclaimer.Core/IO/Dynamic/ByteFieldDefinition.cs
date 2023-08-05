using System.Linq.Expressions;

namespace Reclaimer.IO.Dynamic
{
    internal class ByteFieldDefinition<TClass, TField> : FieldDefinition<TClass, TField>
        where TField : struct, IConvertible
    {
        private delegate TField ReadMethod(EndianReader reader);
        private delegate void WriteMethod(EndianWriter writer, TField value);

        private static readonly ReadMethod InvokeRead;
        private static readonly WriteMethod InvokeWrite;

        static ByteFieldDefinition()
        {
            var typeCode = Type.GetTypeCode(typeof(TField));

            InvokeRead = typeCode switch
            {
                TypeCode.Boolean => CreateReadDelegate(r => r.ReadBoolean()),
                TypeCode.Char => CreateReadDelegate(r => r.ReadChar()),
                TypeCode.SByte => CreateReadDelegate(r => r.ReadSByte()),
                TypeCode.Byte => CreateReadDelegate(r => r.ReadByte()),
                _ => throw new NotSupportedException()
            };

            InvokeWrite = typeCode switch
            {
                TypeCode.Boolean => CreateWriteDelegate<bool>((w, v) => w.Write(v)),
                TypeCode.Char => CreateWriteDelegate<char>((w, v) => w.Write(v)),
                TypeCode.SByte => CreateWriteDelegate<sbyte>((w, v) => w.Write(v)),
                TypeCode.Byte => CreateWriteDelegate<byte>((w, v) => w.Write(v)),
                _ => throw new NotSupportedException()
            };

            static ReadMethod CreateReadDelegate<TRead>(Expression<Func<EndianReader, TRead>> expression)
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

            static WriteMethod CreateWriteDelegate<TWrite>(Expression<Action<EndianWriter, TWrite>> expression)
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

        protected override TField StreamRead(EndianReader reader, ByteOrder byteOrder) => InvokeRead(reader);
        protected override void StreamWrite(EndianWriter writer, TField value, ByteOrder byteOrder) => InvokeWrite(writer, value);
    }
}
