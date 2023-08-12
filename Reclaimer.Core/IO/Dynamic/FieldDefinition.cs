using System.Diagnostics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal abstract class FieldDefinition<TClass>
    {
        public delegate void FieldReadMethod(ref TClass target, EndianReader reader, in ByteOrder? byteOrder);
        public delegate void FieldWriteMethod(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder);

        public readonly FieldReadMethod ReadValue;
        public readonly FieldWriteMethod WriteValue;

        public PropertyInfo TargetProperty { get; }
        public long Offset { get; }
        public ByteOrder? ByteOrder { get; }

        protected FieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            TargetProperty = targetProperty;
            Offset = offset;
            ByteOrder = byteOrder;

            Configure(out ReadValue, out WriteValue);
        }

        protected abstract void Configure(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc);

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

            //since we have a type object that may not necessarily be TClass, we need to construct the appropriate generic method with reflection.
            //rather than make a generic class and find the constructor, its easier to just forward it to the methods below so they can use the generic type params.
            var methodName = DelegateHelper.IsTypeSupported(storeType) ? nameof(CreatePrimitive) : nameof(CreateDynamic);
            var methodInfo = typeof(FieldDefinition<TClass>)
                .GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(storeType);

            return (FieldDefinition<TClass>)methodInfo.Invoke(null, new object[] { targetProperty, offset, byteOrder });
        }

        private static FieldDefinition<TClass> CreatePrimitive<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            return new PrimitiveFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
        }

        private static FieldDefinition<TClass> CreateDynamic<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            return new DynamicFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
        }
    }

    internal abstract class FieldDefinition<TClass, TField> : FieldDefinition<TClass>
    {
        private delegate TField ReferenceTypeGetter(TClass target);
        private delegate void ReferenceTypeSetter(TClass target, TField value);

        private delegate TField ValueTypeGetter(ref TClass target);
        private delegate void ValueTypeSetter(ref TClass target, TField value);

        protected FieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, offset, byteOrder)
        { }

        protected override void Configure(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc)
        {
            if (typeof(TClass).IsValueType)
                ConfigureValueType(out readFunc, out writeFunc);
            else
                ConfigureReferenceType(out readFunc, out writeFunc);
        }

        private void ConfigureReferenceType(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc)
        {
            var isNullable = TargetProperty.PropertyType.IsGenericType && TargetProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

            readFunc = MakeReadFunc();
            writeFunc = MakeWriteFunc();

            FieldReadMethod MakeReadFunc()
            {
                var invokeSetter = MakeSetterFunc();
                return ReferenceTypeRead;

                void ReferenceTypeRead(ref TClass target, EndianReader reader, in ByteOrder? byteOrder)
                {
                    var value = StreamRead(reader, byteOrder);
                    invokeSetter(target, value);
                }
            }

            FieldWriteMethod MakeWriteFunc()
            {
                var invokeGetter = MakeGetterFunc();
                return ReferenceTypeWrite;

                void ReferenceTypeWrite(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder)
                {
                    var value = invokeGetter(target);
                    StreamWrite(writer, value, byteOrder);
                }
            }

            ReferenceTypeGetter MakeGetterFunc()
            {
                return SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField))
                    ? isNullable ? GetViaNullable : TargetProperty.GetMethod.CreateDelegate<ReferenceTypeGetter>()
                    : GetViaConversion;

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

            ReferenceTypeSetter MakeSetterFunc()
            {
                return SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField))
                    ? isNullable ? SetViaNullable : TargetProperty.SetMethod.CreateDelegate<ReferenceTypeSetter>()
                    : SetViaConversion;

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
        }

        private void ConfigureValueType(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc)
        {
            var isNullable = TargetProperty.PropertyType.IsGenericType && TargetProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

            readFunc = MakeReadFunc();
            writeFunc = MakeWriteFunc();

            FieldReadMethod MakeReadFunc()
            {
                var invokeSetter = MakeSetterFunc();
                return ValueTypeRead;

                void ValueTypeRead(ref TClass target, EndianReader reader, in ByteOrder? byteOrder)
                {
                    var value = StreamRead(reader, byteOrder);
                    invokeSetter(ref target, value);
                }
            }

            FieldWriteMethod MakeWriteFunc()
            {
                var invokeGetter = MakeGetterFunc();
                return ValueTypeWrite;

                void ValueTypeWrite(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder)
                {
                    var value = invokeGetter(ref target);
                    StreamWrite(writer, value, byteOrder);
                }
            }

            ValueTypeGetter MakeGetterFunc()
            {
                return SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField))
                    ? isNullable ? GetViaNullable : TargetProperty.GetMethod.CreateDelegate<ValueTypeGetter>()
                    : GetViaConversion;

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

            ValueTypeSetter MakeSetterFunc()
            {
                return SupportsDirectAssignment(TargetProperty.PropertyType, typeof(TField))
                    ? isNullable ? SetViaNullable : TargetProperty.SetMethod.CreateDelegate<ValueTypeSetter>()
                    : SetViaConversion;

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
        }

        protected abstract TField StreamRead(EndianReader reader, in ByteOrder? byteOrder);
        protected abstract void StreamWrite(EndianWriter writer, TField value, in ByteOrder? byteOrder);

        protected override string GetDebuggerDisplay() => $"@{Offset,4} (0x{Offset:X4}) : [{typeof(TField).Name}] {typeof(TClass).Name}.{TargetProperty.Name}";
    }
}
