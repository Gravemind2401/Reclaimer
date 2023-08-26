using System.Diagnostics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    /// <summary>
    /// The base class for all field definitions, regardless of property type.
    /// <br/>There is no generic type parameter for the field type, allowing definitions for different property types to be added to the same list.
    /// </summary>
    /// <typeparam name="TClass">The <see langword="class"/> or <see langword="struct"/> that the property is a member of.</typeparam>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal abstract class FieldDefinition<TClass>
    {
        public delegate void FieldReadMethod(ref TClass target, EndianReader reader, in ByteOrder? byteOrder);
        public delegate void FieldWriteMethod(ref TClass target, EndianWriter writer, in ByteOrder? byteOrder);

        /// <summary>
        /// Reads the property value from stream and stores it in the target property of the target object.
        /// </summary>
        public readonly FieldReadMethod ReadValue;

        /// <summary>
        /// Gets the value from the target property of the target object and writes it to stream.
        /// </summary>
        public readonly FieldWriteMethod WriteValue;

        /// <summary>
        /// The type used to store the property value in a stream.
        /// <br/>This may not necessarily be the same as the property type.
        /// </summary>
        public Type TargetType { get; }

        public PropertyInfo TargetProperty { get; }
        public long Offset { get; }
        public ByteOrder? ByteOrder { get; }
        public bool IsVersionProperty { get; init; }
        public bool IsDataLengthProperty { get; init; }

        protected FieldDefinition(PropertyInfo targetProperty, Type targetType, long offset, ByteOrder? byteOrder)
        {
            TargetProperty = targetProperty;
            TargetType = targetType;
            Offset = offset;
            ByteOrder = byteOrder;

            Configure(out ReadValue, out WriteValue);
        }

        protected abstract void Configure(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc);

        /// <summary>
        /// Reads the target value from the stream and converts it to a <see cref="double"/>.
        /// <br/>Does not set the value against the target property.
        /// <br/>Only to be used when the target property has <see cref="VersionNumberAttribute"/> applied.
        /// </summary>
        internal abstract double StreamReadVersionField(EndianReader reader, in ByteOrder? byteOrder);

        /// <summary>
        /// Converts the version number to the target property type and writes it to stream as if it was the value of the target property.
        /// <br/>Only to be used when the target property has <see cref="VersionNumberAttribute"/> applied.
        /// </summary>
        internal abstract void StreamWriteVersionField(EndianWriter writer, double value, in ByteOrder? byteOrder);

        protected virtual string GetDebuggerDisplay() => $"0x{Offset:X4} / {Offset:D4} : [{TargetType.Name}] {typeof(TClass).Name}.{TargetProperty.Name}";

        protected static bool SupportsDirectAssignment(Type type1, Type type2) => Utils.GetUnderlyingType(type1) == Utils.GetUnderlyingType(type2);

        public static FieldDefinition<TClass> Create(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder, Type storeType)
        {
            storeType = Utils.GetUnderlyingType(storeType ?? targetProperty.PropertyType);

            if (storeType == typeof(string))
                return StringFieldDefinition<TClass>.FromAttributes(targetProperty, offset, byteOrder);

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
            return new PrimitiveFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder)
            {
                IsVersionProperty = IsVersionProperty(),
                IsDataLengthProperty = IsDataLengthProperty()
            };

            bool IsVersionProperty()
            {
                if (!Attribute.IsDefined(targetProperty, typeof(VersionNumberAttribute)))
                    return false;

                VersionNumberAttribute.ThrowIfInvalidPropertyType(typeof(TField));
                return true;
            }

            bool IsDataLengthProperty()
            {
                if (!Attribute.IsDefined(targetProperty, typeof(DataLengthAttribute)))
                    return false;

                DataLengthAttribute.ThrowIfInvalidPropertyType(typeof(TField));
                return true;
            }
        }

        private static FieldDefinition<TClass> CreateDynamic<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
        {
            return new DynamicFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
        }
    }

    /// <summary>
    /// The base class for all strongly typed field definitions.
    /// </summary>
    /// <typeparam name="TClass">The <see langword="class"/> or <see langword="struct"/> that the property is a member of.</typeparam>
    /// <typeparam name="TField">
    /// The type used to store the property value in a stream.
    /// <br/>This may not necessarily be the same as the property type.
    /// </typeparam>
    internal abstract class FieldDefinition<TClass, TField> : FieldDefinition<TClass>
    {
        private delegate TField ReferenceTypeGetter(TClass target);
        private delegate void ReferenceTypeSetter(TClass target, TField value);

        private delegate TField ValueTypeGetter(ref TClass target);
        private delegate void ValueTypeSetter(ref TClass target, TField value);

        protected FieldDefinition(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            : base(targetProperty, typeof(TField), offset, byteOrder)
        { }

        protected sealed override void Configure(out FieldReadMethod readFunc, out FieldWriteMethod writeFunc)
        {
            if (typeof(TClass).IsValueType)
                ConfigureValueType(out readFunc, out writeFunc);
            else
                ConfigureReferenceType(out readFunc, out writeFunc);
        }

        //when TClass is a class
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
                        throw Exceptions.PropertyNotConvertable(TargetProperty, typeof(TField));
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
                        throw Exceptions.PropertyNotConvertable(TargetProperty, typeof(TField));
                    TargetProperty.SetValue(obj, converted);
                }
            }
        }

        //when TClass is a struct
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

        internal sealed override double StreamReadVersionField(EndianReader reader, in ByteOrder? byteOrder)
        {
            return Convert.ToDouble(StreamRead(reader, byteOrder));
        }

        internal sealed override void StreamWriteVersionField(EndianWriter writer, double value, in ByteOrder? byteOrder)
        {
            StreamWrite(writer, (TField)Convert.ChangeType(value, typeof(TField)), byteOrder);
        }

        protected abstract TField StreamRead(EndianReader reader, in ByteOrder? byteOrder);
        protected abstract void StreamWrite(EndianWriter writer, TField value, in ByteOrder? byteOrder);
    }
}
