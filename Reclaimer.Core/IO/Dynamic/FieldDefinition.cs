using System.Diagnostics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal abstract class FieldDefinition<TClass>
    {
        public virtual PropertyInfo TargetProperty { get; init; }
        public long Offset { get; init; }
        public ByteOrder? ByteOrder { get; init; }

        public abstract void ReadValue(TClass target, EndianReader reader, ByteOrder byteOrder);
        public abstract void WriteValue(TClass target, EndianWriter writer, ByteOrder byteOrder);

        protected virtual string GetDebuggerDisplay() => $"@{Offset,4} (0x{Offset:X4}) : [{TargetProperty.PropertyType.Name}] {typeof(TClass).Name}.{TargetProperty.Name}";

        public static FieldDefinition<TClass> Create(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder, Type storeType)
        {
            storeType ??= targetProperty.PropertyType;

            if (storeType.IsGenericType && storeType.GetGenericTypeDefinition() == typeof(Nullable<>))
                storeType = storeType.GetGenericArguments()[0];

            if (storeType == typeof(string))
            {
                return new StringFieldDefinition<TClass>
                {
                    TargetProperty = targetProperty,
                    Offset = offset,
                    ByteOrder = byteOrder
                };
            }

            if (storeType == typeof(Half))
            {
                return new HalfFieldDefinition<TClass>
                {
                    TargetProperty = targetProperty,
                    Offset = offset,
                    ByteOrder = byteOrder
                };
            }

            string methodName;
            if (storeType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBufferable<>) && i.GetGenericArguments().SequenceEqual(Enumerable.Repeat(storeType, 1))))
                methodName = nameof(CreateBufferable);
            else
            {
                var typeCode = Type.GetTypeCode(storeType);
                if (typeCode is TypeCode.Boolean or TypeCode.Char or TypeCode.SByte or TypeCode.Byte)
                    methodName = nameof(CreateByte);
                else
                    methodName = nameof(CreatePrimitive);
            }

            var methodInfo = typeof(FieldDefinition<TClass>)
                .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(storeType);

            return (FieldDefinition<TClass>)methodInfo.Invoke(null, new object[] { targetProperty, offset, byteOrder });
        }

        private static FieldDefinition<TClass> CreateBufferable<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : IBufferable<TField>
        {
            return new BufferableFieldDefinition<TClass, TField>
            {
                TargetProperty = targetProperty,
                Offset = offset,
                ByteOrder = byteOrder
            };
        }

        private static FieldDefinition<TClass> CreateByte<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : struct, IConvertible
        {
            return new ByteFieldDefinition<TClass, TField>
            {
                TargetProperty = targetProperty,
                Offset = offset,
                ByteOrder = byteOrder
            };
        }

        private static FieldDefinition<TClass> CreatePrimitive<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : struct, IConvertible
        {
            return new PrimitiveFieldDefinition<TClass, TField>
            {
                TargetProperty = targetProperty,
                Offset = offset,
                ByteOrder = byteOrder
            };
        }

        private static FieldDefinition<TClass> CreateNullable<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : struct, IConvertible
        {
            return new PrimitiveFieldDefinition<TClass, TField>
            {
                TargetProperty = targetProperty,
                Offset = offset,
                ByteOrder = byteOrder
            };
        }
    }

    internal abstract class FieldDefinition<TClass, TField> : FieldDefinition<TClass>
    {
        private delegate TField GetMethod(TClass target);
        private delegate void SetMethod(TClass target, TField value);

        private GetMethod InvokeGet { get; init; }
        private SetMethod InvokeSet { get; init; }

        public override PropertyInfo TargetProperty
        {
            get => base.TargetProperty;
            init
            {
                base.TargetProperty = value;
                InvokeGet = CreateGetterDelegate();
                InvokeSet = CreateSetterDelegate();
            }
        }

        public override void ReadValue(TClass target, EndianReader reader, ByteOrder byteOrder)
        {
            var value = StreamRead(reader, byteOrder);
            InvokeSet(target, value);
        }

        public override void WriteValue(TClass target, EndianWriter writer, ByteOrder byteOrder)
        {
            var value = InvokeGet(target);
            StreamWrite(writer, value, byteOrder);
        }

        private GetMethod CreateGetterDelegate()
        {
            if (TargetProperty.PropertyType == typeof(TField))
                return TargetProperty.GetMethod.CreateDelegate<GetMethod>();

            return (obj) =>
            {
                var value = TargetProperty.GetValue(obj);
                if (!Utils.TryConvert(ref value, TargetProperty.PropertyType, typeof(TField)))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                return (TField)value;
            };
        }

        private SetMethod CreateSetterDelegate()
        {
            if (TargetProperty.PropertyType == typeof(TField))
                return TargetProperty.SetMethod.CreateDelegate<SetMethod>();

            return (obj, value) =>
            {
                var converted = (object)value;
                if (!Utils.TryConvert(ref converted, typeof(TField), TargetProperty.PropertyType))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                TargetProperty.SetValue(obj, converted);
            };
        }

        protected abstract TField StreamRead(EndianReader reader, ByteOrder byteOrder);
        protected abstract void StreamWrite(EndianWriter writer, TField value, ByteOrder byteOrder);

        protected override string GetDebuggerDisplay() => $"@{Offset,4} (0x{Offset:X4}) : [{typeof(TField).Name}] {typeof(TClass).Name}.{TargetProperty.Name}";
    }
}
