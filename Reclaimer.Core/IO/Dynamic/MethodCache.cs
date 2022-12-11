using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public delegate object ReadMethod(EndianReader reader, ByteOrder byteOrder);
    public delegate void WriteMethod(EndianWriter writer, ByteOrder byteOrder, object value);

    internal static class MethodCache
    {
        private static readonly IDictionary<Type, ReadMethod> readMethodCache = GetPrimitiveReadMethods();
        private static readonly IDictionary<Type, WriteMethod> writeMethodCache = GetPrimitiveWriteMethods();

        private static Dictionary<Type, ReadMethod> GetPrimitiveReadMethods()
        {
            var lookup = typeof(EndianReader)
                .GetMethods()
                .Where(IsPrimitiveRead)
                .ToDictionary(m => m.ReturnType, CreateDelegate);

            foreach (var type in lookup.Keys.ToList())
                lookup.Add(typeof(Nullable<>).MakeGenericType(type), lookup[type]);

            return lookup;

            static bool IsPrimitiveRead(MethodInfo m)
            {
                if (!m.ReturnType.IsValueType || !m.Name.StartsWith(nameof(EndianReader.Read)))
                    return false;

                var param = m.GetParameters();
                return param.Length == 0
                    ? m.ReturnType == typeof(byte) || m.ReturnType == typeof(sbyte)
                    : param.Length == 1 && param[0].ParameterType == typeof(ByteOrder);
            }

            static ReadMethod CreateDelegate(MethodInfo m)
            {
                return m.GetParameters().Length == 0
                    ? (reader, byteOrder) => m.Invoke(reader, null)
                    : (reader, byteOrder) => m.Invoke(reader, new object[] { byteOrder });
            }
        }

        private static Dictionary<Type, WriteMethod> GetPrimitiveWriteMethods()
        {
            var lookup = typeof(EndianWriter)
                .GetMethods()
                .Where(IsPrimitiveWrite)
                .ToDictionary(m => m.GetParameters()[0].ParameterType, CreateDelegate);

            foreach (var type in lookup.Keys.ToList())
                lookup.Add(typeof(Nullable<>).MakeGenericType(type), lookup[type]);

            return lookup;

            static bool IsPrimitiveWrite(MethodInfo m)
            {
                if (m.ReturnType != typeof(void) || !m.Name.Equals(nameof(EndianWriter.Write)))
                    return false;

                var param = m.GetParameters();
                return param.Length == 1
                    ? param[0].ParameterType == typeof(byte) || param[0].ParameterType == typeof(sbyte)
                    : param.Length == 2 && param[0].ParameterType.IsValueType && param[1].ParameterType == typeof(ByteOrder);
            }

            static WriteMethod CreateDelegate(MethodInfo m)
            {
                return m.GetParameters().Length == 1
                    ? (writer, byteOrder, value) => m.Invoke(writer, new object[] { value })
                    : (writer, byteOrder, value) => m.Invoke(writer, new object[] { value, byteOrder });
            }
        }

        public static bool TryGetReadMethod(Type type, out ReadMethod readMethod)
        {
            if (readMethodCache.TryGetValue(type,out readMethod))
                return true;

            var bufferableType = typeof(IBufferable<>).MakeGenericType(type);
            if (!bufferableType.IsAssignableFrom(type))
                return false;

            var readBufferable = typeof(EndianReader).GetMethods()
                .Where(m => m.Name == nameof(EndianReader.ReadBufferable) && m.GetParameters().Length == 1)
                .Single().MakeGenericMethod(type);

            readMethod = (reader, byteOrder) => readBufferable.Invoke(reader, new object[] { byteOrder });
            readMethodCache.Add(type, readMethod);

            return true;
        }

        public static bool TryGetWriteMethod(Type type, out WriteMethod writeMethod)
        {
            if (writeMethodCache.TryGetValue(type, out writeMethod))
                return true;

            if (!typeof(IBufferable).IsAssignableFrom(type))
                return false;

            var writeBufferable = typeof(EndianWriter).GetMethods()
                .Where(m => m.Name == nameof(EndianWriter.WriteBufferable) && m.GetParameters().Length == 2)
                .Single().MakeGenericMethod(type);

            writeMethod = (writer, byteOrder, value) => writeBufferable.Invoke(writer, new object[] { value, byteOrder });
            writeMethodCache.Add(type, writeMethod);

            return true;
        }
    }
}
