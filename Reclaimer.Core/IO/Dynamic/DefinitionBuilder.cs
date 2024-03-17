using System.Linq.Expressions;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public class DefinitionBuilder<TClass>
    {
        private readonly List<VersionBuilder> versions = new();

        internal IEnumerable<VersionBuilder> GetVersions() => versions.AsEnumerable();

        public VersionBuilder AddDefaultVersion() => AddVersionNumeric(null, null);

        public VersionBuilder AddVersion<TNumber>(TNumber version)
            where TNumber : struct, INumber<TNumber>
            => AddVersionNumeric(Convert.ToInt32(version), Convert.ToInt32(version));

        public VersionBuilder AddVersion<TNumber>(TNumber? minVersion, TNumber? maxVersion)
            where TNumber : struct, INumber<TNumber>
            => AddVersionNumeric(minVersion.HasValue ? Convert.ToInt32(minVersion) : null, maxVersion.HasValue ? Convert.ToInt32(maxVersion) : null);

        public VersionBuilder AddVersion(Enum version) => AddVersionEnum(version, version);
        public VersionBuilder AddVersion(Enum minVersion, Enum maxVersion) => AddVersionEnum(minVersion, maxVersion);

        private VersionBuilder AddVersionNumeric(double? minVersion, double? maxVersion)
        {
            var version = new VersionBuilder(minVersion, maxVersion);
            versions.Add(version);
            return version;
        }

        private VersionBuilder AddVersionEnum(Enum minVersion, Enum maxVersion)
        {
            var version = new VersionBuilder(minVersion, maxVersion);
            versions.Add(version);
            return version;
        }

        internal StructureDefinition<TClass> ToStructureDefinition()
        {
            return new StructureDefinition<TClass>(versions.Select(v => v.ToVersionDefinition()));
        }

        public sealed class VersionBuilder
        {
            private readonly Dictionary<PropertyInfo, FieldBuilderBase> fields = new();

            private readonly double? minVersion;
            private readonly double? maxVersion;

            private readonly string minVersionDisplay;
            private readonly string maxVersionDisplay;

            private ByteOrder? byteOrder;
            private int? size;

            internal VersionBuilder(double? minVersion, double? maxVersion)
            {
                this.minVersion = minVersion;
                this.maxVersion = maxVersion;
            }

            internal VersionBuilder(Enum minVersion, Enum maxVersion)
                : this(minVersion == null ? null : Convert.ToInt32(minVersion), maxVersion == null ? null : Convert.ToInt32(maxVersion))
            {
                minVersionDisplay = minVersion?.ToString();
                maxVersionDisplay = maxVersion?.ToString();
            }

            /// <summary>
            /// Specifies that an object always takes up the same number of bytes when stored in a stream.
            /// </summary>
            /// <param name="size">The number of bytes used to store the object.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public VersionBuilder HasFixedSize(int size)
            {
                this.size = size;
                return this;
            }

            /// <summary>
            /// Specifies that all properties of the object should be stored using a specific byte order.
            /// </summary>
            /// <param name="byteOrder">The byte order to use when reading and writing this type's properties.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public VersionBuilder HasByteOrder(ByteOrder byteOrder)
            {
                this.byteOrder = byteOrder;
                return this;
            }

            //while TProperty could be primitive OR dynamic, the generic constraints cant be overloaded and we can only return one type
            //even if we added overloads for every possible primtive type (like the string overload below) this wouldnt account for IBufferable types
            //so PrimitiveFieldBuilder will therefore have to handle both primitive and dynamic fields
            public PrimitiveFieldBuilder Property<TProperty>(Expression<Func<TClass, TProperty>> propertyExpression)
            {
                var property = Utils.PropertyFromExpression(propertyExpression);
                if (!fields.TryGetValue(property, out var map))
                    fields.Add(property, map = new PrimitiveFieldBuilder(property));
                return (PrimitiveFieldBuilder)map;
            }

            public StringFieldBuilder Property(Expression<Func<TClass, string>> propertyExpression)
            {
                var property = Utils.PropertyFromExpression(propertyExpression);
                if (!fields.TryGetValue(property, out var map))
                    fields.Add(property, map = new StringFieldBuilder(property));
                return (StringFieldBuilder)map;
            }

            internal StructureDefinition<TClass>.VersionDefinition ToVersionDefinition()
            {
                var versionDef = new StructureDefinition<TClass>.VersionDefinition(minVersion, maxVersion, byteOrder, size, minVersionDisplay, maxVersionDisplay);

                foreach (var fieldInfo in fields.Values)
                    versionDef.AddField(fieldInfo.GetFieldDefinition());

                return versionDef;
            }
        }

        public abstract class FieldBuilderBase
        {
            internal PropertyInfo TargetProperty { get; }

            internal long? Offset { get; private protected set; }
            internal ByteOrder? ByteOrder { get; private protected set; }

            internal FieldBuilderBase(PropertyInfo targetProperty)
            {
                TargetProperty = targetProperty;
            }

            internal abstract FieldDefinition<TClass> GetFieldDefinition();
        }

        public abstract class FieldBuilderBase<TSelf> : FieldBuilderBase
            where TSelf : FieldBuilderBase<TSelf>
        {
            internal FieldBuilderBase(PropertyInfo targetProperty)
                : base(targetProperty)
            { }

            /// <summary>
            /// Specifies the offset of this property within the stream, relative to the beginning of its containing type.
            /// </summary>
            /// <param name="offset">The offset of the property's value, in bytes.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public TSelf HasOffset(long offset)
            {
                Offset = offset;
                return (TSelf)this;
            }

            /// <summary>
            /// Specifies that this property should be stored using a specific byte order.
            /// </summary>
            /// <param name="byteOrder">The byte order to use when reading and writing this property.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public TSelf HasByteOrder(ByteOrder byteOrder)
            {
                ByteOrder = byteOrder;
                return (TSelf)this;
            }
        }

        public sealed class PrimitiveFieldBuilder : FieldBuilderBase<PrimitiveFieldBuilder>
        {
            private Type storeType;
            private bool isVersionNumber;
            private bool isDataLength;

            internal PrimitiveFieldBuilder(PropertyInfo targetProperty)
                : base(targetProperty)
            { }

            /// <summary>
            /// Specifies that the property's value should be converted when being read or written.
            /// </summary>
            /// <param name="storeType">The type used to store the object.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public PrimitiveFieldBuilder StoreType(Type storeType)
            {
                this.storeType = storeType;
                return this;
            }

            /// <summary>
            /// Specifies that the property holds the version number of the object being read or written.
            /// </summary>
            /// <remarks>
            /// This is only valid for integer and floating-point typed properties.
            /// </remarks>
            /// <returns></returns>
            public PrimitiveFieldBuilder IsVersionNumber() => IsVersionNumber(true);

            /// <summary>
            /// Specifies whether the property holds the version number of the object being read or written.
            /// </summary>
            /// <param name="isVersionNumber"><see langword="true"/> if this property represents the object version, otherwise <see langword="false"/>.</param>
            /// <inheritdoc cref="IsVersionNumber()"/>
            public PrimitiveFieldBuilder IsVersionNumber(bool isVersionNumber)
            {
                this.isVersionNumber = isVersionNumber;
                return this;
            }

            /// <summary>
            /// Specifies that the property value represents the number of bytes used to store its containing instance.
            /// </summary>
            /// <remarks>
            /// This is only valid for integer typed properties. The property value must be a positive integer less than <see cref="long.MaxValue"/>.
            /// </remarks>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public PrimitiveFieldBuilder IsDataLength() => IsDataLength(true);

            /// <summary>
            /// Specifies whether the property value represents the number of bytes used to store its containing instance.
            /// </summary>
            /// <param name="isDataLength"><see langword="true"/> if this property represents the object size, otherwise <see langword="false"/>.</param>
            /// <inheritdoc cref="IsDataLength()"/>
            public PrimitiveFieldBuilder IsDataLength(bool isDataLength)
            {
                this.isDataLength = isDataLength;
                return this;
            }

            internal override FieldDefinition<TClass> GetFieldDefinition()
            {
                var storeType = Utils.GetUnderlyingType(this.storeType ?? TargetProperty.PropertyType);

                //since we have a type object that may not necessarily be TClass, we need to construct the appropriate generic method with reflection.
                //rather than make a generic class and find the constructor, its easier to just forward it to the methods below so they can use the generic type params.
                var methodName = DelegateHelper.IsTypeSupported(storeType) ? nameof(CreatePrimitive) : nameof(CreateDynamic);
                var methodInfo = GetType()
                    .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                    .MakeGenericMethod(storeType);

                return (FieldDefinition<TClass>)methodInfo.Invoke(this, new object[] { TargetProperty, Offset.GetValueOrDefault(), ByteOrder });
            }

            private FieldDefinition<TClass> CreatePrimitive<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            {
                return new PrimitiveFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder)
                {
                    IsVersionProperty = IsVersionProperty(),
                    IsDataLengthProperty = IsDataLengthProperty()
                };

                bool IsVersionProperty()
                {
                    if (!isVersionNumber)
                        return false;

                    VersionNumberAttribute.ThrowIfInvalidPropertyType(typeof(TField));
                    return true;
                }

                bool IsDataLengthProperty()
                {
                    if (!isDataLength)
                        return false;

                    DataLengthAttribute.ThrowIfInvalidPropertyType(typeof(TField));
                    return true;
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Access via reflection using instance binding flags")]
            private FieldDefinition<TClass> CreateDynamic<TField>(PropertyInfo targetProperty, long offset, ByteOrder? byteOrder)
            {
                return new DynamicFieldDefinition<TClass, TField>(targetProperty, offset, byteOrder);
            }
        }

        public sealed class StringFieldBuilder : FieldBuilderBase<StringFieldBuilder>
        {
            private bool isInterned;
            private bool isLengthPrefixed;
            private bool isNullTerminated;
            private bool isFixedLength;
            private bool trimEnabled;
            private char paddingChar;
            private int length;

            internal StringFieldBuilder(PropertyInfo targetProperty)
                : base(targetProperty)
            { }

            /// <summary>
            /// Specifies that <see cref="string.Intern(string)"/> should be used when reading the string value.
            /// </summary>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsInterned() => IsInterned(true);

            /// <summary>
            /// Specifies whether <see cref="string.Intern(string)"/> should be used when reading the string value.
            /// </summary>
            /// <param name="isInterned"><see langword="true"/> if the string should be interned, otherwise <see langword="false"/>.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsInterned(bool isInterned)
            {
                this.isInterned = isInterned;
                return this;
            }

            /// <summary>
            /// Specifies that the string property is stored as length-prefixed.
            /// </summary>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsLengthPrefixed()
            {
                isNullTerminated = isFixedLength = false;
                isLengthPrefixed = true;
                return this;
            }

            /// <summary>
            /// Specifies that the string property is stored as fixed-length.
            /// </summary>
            /// <param name="length">The number of characters allocated to store the string.</param>
            /// <param name="trim">Indicates if trailing white-space should be trimmed from the string upon read.</param>
            /// <param name="padding">The character to use as padding if the string is shorter than its assigned length upon write.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsFixedLength(int length, bool trim = false, char padding = ' ')
            {
                isNullTerminated = isLengthPrefixed = false;
                isFixedLength = true;
                this.length = length;
                trimEnabled = trim;
                paddingChar = padding;
                return this;
            }

            /// <summary>
            /// Specifies that the string property is stored as null-terminated.
            /// </summary>
            /// <param name="maxLength">The maximum number of characters that can be stored in the stream.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsNullTerminated() => IsNullTerminated(default);

            /// <summary>
            /// Specifies that the string property is stored as null-terminated, but never longer than the specified length.
            /// </summary>
            /// <param name="maxLength">The maximum number of characters that can be stored in the stream.</param>
            /// <returns><see langword="this"/>, for convenient chaining.</returns>
            public StringFieldBuilder IsNullTerminated(int maxLength)
            {
                isFixedLength = isLengthPrefixed = false;
                isNullTerminated = true;
                length = maxLength;
                return this;
            }

            internal override FieldDefinition<TClass> GetFieldDefinition()
            {
                if (isLengthPrefixed)
                    return StringFieldDefinition<TClass>.LengthPrefixed(TargetProperty, Offset.GetValueOrDefault(), ByteOrder, isInterned);

                if (isFixedLength)
                    return StringFieldDefinition<TClass>.FixedLength(TargetProperty, Offset.GetValueOrDefault(), isInterned, length, paddingChar, trimEnabled);

                if (isNullTerminated)
                    return StringFieldDefinition<TClass>.NullTerminated(TargetProperty, Offset.GetValueOrDefault(), isInterned, length);

                throw Exceptions.StringTypeUnknown(TargetProperty);
            }
        }
    }
}
