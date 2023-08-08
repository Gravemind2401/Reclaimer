using System.Diagnostics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal abstract class FieldDefinition<TClass>
    {
        public PropertyInfo TargetProperty { get; }
        public long Offset { get; }
        public ByteOrder? ByteOrder { get; }

        protected FieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            TargetProperty = targetProperty;
            Offset = offset;
            ByteOrder = byteOrder;
        }

        public abstract void ReadValue(ref TClass target, EndianReader reader, in ByteOrder? byteOrder);
        public abstract void WriteValue(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder);

        protected virtual string GetDebuggerDisplay() => $"@{Offset,4} (0x{Offset:X4}) : [{TargetProperty.PropertyType.Name}] {typeof(TClass).Name}.{TargetProperty.Name}";

        private static Type GetUnderlyingType(Type storeType)
        {
            if (storeType.IsGenericType && storeType.GetGenericTypeDefinition() == typeof(Nullable<>))
                storeType = storeType.GetGenericArguments()[0];

            if (storeType.IsEnum)
                storeType = Enum.GetUnderlyingType(storeType);

            return storeType;
        }

        protected static bool SupportsDirectAssignment(Type type1, Type type2) => GetUnderlyingType(type1) == GetUnderlyingType(type2);

        public static FieldDefinition<TClass> Create(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder, Type storeType)
        {
            storeType = GetUnderlyingType(storeType ?? targetProperty.PropertyType);

            if (storeType == typeof(string))
                return new StringFieldDefinition<TClass>(targetProperty, offset, byteOrder);

            var methodInfo = typeof(FieldDefinition<TClass>)
                .GetMethod(nameof(CreatePrimitive), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(storeType);

            return (FieldDefinition<TClass>)methodInfo.Invoke(null, new object[] { targetProperty, offset, byteOrder });
        }

        private static FieldDefinition<TClass> CreatePrimitive<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : struct
        {
            return new PrimitiveFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
        }

        private static FieldDefinition<TClass> CreateNullable<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            where TField : struct
        {
            return new PrimitiveFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
        }
    }

    internal abstract class FieldDefinition<TClass, TField> : FieldDefinition<TClass>
    {
        private delegate TField ReferenceTypeGetMethod(TClass target);
        private delegate void ReferenceTypeSetMethod(TClass target, TField value);

        private delegate TField ValueTypeGetMethod(ref TClass target);
        private delegate void ValueTypeSetMethod(ref TClass target, TField value);

        private readonly bool isNullable;

        private readonly ReferenceTypeGetMethod InvokeReferenceTypeGet;
        private readonly ReferenceTypeSetMethod InvokeReferenceTypeSet;

        private readonly ValueTypeGetMethod InvokeValueTypeGet;
        private readonly ValueTypeSetMethod InvokeValueTypeSet;

        protected FieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, offset, byteOrder)
        {
            isNullable = TargetProperty.PropertyType.IsGenericType && TargetProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (typeof(TClass).IsValueType)
            {
                InvokeValueTypeGet = CreateValueTypeGetterDelegate();
                InvokeValueTypeSet = CreateValueTypeSetterDelegate();
            }
            else
            {
                InvokeReferenceTypeGet = CreateReferenceTypeGetterDelegate();
                InvokeReferenceTypeSet = CreateReferenceTypeSetterDelegate();
            }
        }

        public override void ReadValue(ref TClass target, EndianReader reader, in ByteOrder? byteOrder)
        {
            var value = StreamRead(reader, byteOrder);

            if (typeof(TClass).IsValueType)
                InvokeValueTypeSet(ref target, value);
            else
                InvokeReferenceTypeSet(target, value);
        }

        public override void WriteValue(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder)
        {
            var value = typeof(TClass).IsValueType
                ? InvokeValueTypeGet(ref target)
                : InvokeReferenceTypeGet(target);

            StreamWrite(writer, value, byteOrder);
        }

        private ReferenceTypeGetMethod CreateReferenceTypeGetterDelegate()
        {
            if (SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField)))
            {
                if (isNullable)
                    return GetViaNullable;

                return TargetProperty.GetMethod.CreateDelegate<ReferenceTypeGetMethod>();
            }

            return GetViaConversion;

            TField GetViaNullable(TClass obj)
            {
                //same net result as {return obj.Property.GetValueOrDefault()}
                return TargetProperty.GetValue(obj) is TField result ? result : default;
            }

            TField GetViaConversion(TClass obj)
            {
                var value = TargetProperty.GetValue(obj);
                if (!Utils.TryConvert(ref value, TargetProperty.PropertyType, typeof(TField)))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                return (TField)value;
            }
        }

        private ReferenceTypeSetMethod CreateReferenceTypeSetterDelegate()
        {
            if (SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField)))
            {
                if (isNullable)
                    return SetViaNullable;

                return TargetProperty.SetMethod.CreateDelegate<ReferenceTypeSetMethod>();
            }

            return SetViaConversion;

            void SetViaNullable(TClass obj, TField value)
            {
                //delegate isnt compatable, but it can be set via SetValue with boxing
                TargetProperty.SetValue(obj, value);
            }

            void SetViaConversion(TClass obj, TField value)
            {
                var converted = (object)value;
                if (!Utils.TryConvert(ref converted, typeof(TField), TargetProperty.PropertyType))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                TargetProperty.SetValue(obj, converted);
            }
        }

        private ValueTypeGetMethod CreateValueTypeGetterDelegate()
        {
            if (SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField)))
            {
                if (isNullable)
                    return GetViaNullable;

                return TargetProperty.GetMethod.CreateDelegate<ValueTypeGetMethod>();
            }

            return GetViaConversion;

            TField GetViaNullable(ref TClass obj)
            {
                //same net result as {return obj.Property.GetValueOrDefault()}
                return TargetProperty.GetValue(obj) is TField result ? result : default;
            }

            TField GetViaConversion(ref TClass obj)
            {
                var value = TargetProperty.GetValue(obj);
                if (!Utils.TryConvert(ref value, TargetProperty.PropertyType, typeof(TField)))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                return (TField)value;
            }
        }

        private ValueTypeSetMethod CreateValueTypeSetterDelegate()
        {
            if (SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField)))
            {
                if (isNullable)
                    return SetViaNullable;

                return TargetProperty.SetMethod.CreateDelegate<ValueTypeSetMethod>();
            }

            return SetViaConversion;

            void SetViaNullable(ref TClass obj, TField value)
            {
                //delegate isnt compatable, but it can be set via SetValue with boxing
                TargetProperty.SetValue(obj, value);
            }

            void SetViaConversion(ref TClass obj, TField value)
            {
                var converted = (object)value;
                if (!Utils.TryConvert(ref converted, typeof(TField), TargetProperty.PropertyType))
                    throw new InvalidCastException($"The value in {TargetProperty.Name} could not be stored as {typeof(TField)}");
                TargetProperty.SetValue(obj, converted);
            }
        }

        protected abstract TField StreamRead(EndianReader reader, in ByteOrder? byteOrder);
        protected abstract void StreamWrite(EndianWriter writer, TField value, in ByteOrder? byteOrder);

        protected override string GetDebuggerDisplay() => $"@{Offset,4} (0x{Offset:X4}) : [{typeof(TField).Name}] {typeof(TClass).Name}.{TargetProperty.Name}";
    }
}
