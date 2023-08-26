using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public class DefinitionBuilder<TClass>
    {
        private readonly List<VersionBuilder> versions = new();

        internal IEnumerable<VersionBuilder> GetVersions() => versions.AsEnumerable();

        public VersionBuilder AddDefaultVersion() => AddVersionInternal(null, null);

        public VersionBuilder AddVersion<TNumber>(TNumber version)
            where TNumber : struct, INumber<TNumber>
            => AddVersionInternal(Convert.ToInt32(version), Convert.ToInt32(version));

        public VersionBuilder AddVersion<TNumber>(TNumber? minVersion, TNumber? maxVersion)
            where TNumber : struct, INumber<TNumber>
            => AddVersionInternal(minVersion.HasValue ? Convert.ToInt32(minVersion) : null, maxVersion.HasValue ? Convert.ToInt32(maxVersion) : null);

        public VersionBuilder AddVersion(Enum version) => AddVersionInternal(Convert.ToInt32(version), Convert.ToInt32(version));
        public VersionBuilder AddVersion(Enum minVersion, Enum maxVersion) => AddVersionInternal(minVersion == null ? null : Convert.ToInt32(minVersion), maxVersion == null ? null : Convert.ToInt32(maxVersion));

        private VersionBuilder AddVersionInternal(double? minVersion, double? maxVersion)
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

            private ByteOrder? byteOrder;
            private int? size;

            internal VersionBuilder(double? minVersion, double? maxVersion)
            {
                this.minVersion = minVersion;
                this.maxVersion = maxVersion;
            }

            public VersionBuilder HasFixedSize(int size)
            {
                this.size = size;
                return this;
            }

            public VersionBuilder HasByteOrder(ByteOrder byteOrder)
            {
                this.byteOrder = byteOrder;
                return this;
            }

            //TODO: differentiate between primitive and dynamic
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
                var versionDef = new StructureDefinition<TClass>.VersionDefinition(minVersion, maxVersion, byteOrder, size);

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

            public TSelf HasOffset(long offset)
            {
                Offset = offset;
                return (TSelf)this;
            }

            public TSelf HasByteOrder(ByteOrder byteOrder)
            {
                ByteOrder = byteOrder;
                return (TSelf)this;
            }
        }

        public sealed class PrimitiveFieldBuilder : FieldBuilderBase<PrimitiveFieldBuilder>
        {
            private Type storeType;
            private bool isDataLength; //TODO: apply this to resulting field definition
            
            internal PrimitiveFieldBuilder(PropertyInfo targetProperty)
                : base(targetProperty)
            { }

            public PrimitiveFieldBuilder StoreType(Type storeType)
            {
                this.storeType = storeType;
                return this;
            }

            public PrimitiveFieldBuilder IsDataLength() => IsDataLength(true);
            public PrimitiveFieldBuilder IsDataLength(bool isDataLength)
            {
                this.isDataLength = isDataLength;
                return this;
            }

            internal override FieldDefinition<TClass> GetFieldDefinition()
            {
                //not creating definition directly because TField may differ depending on StoreType
                //and that logic is already handled in FieldDefinition<TClass>.Create()
                return FieldDefinition<TClass>.Create(TargetProperty, Offset.GetValueOrDefault(), ByteOrder, storeType);
            }
        }

        public sealed class DynamicFieldBuilder : FieldBuilderBase<DynamicFieldBuilder>
        {
            internal DynamicFieldBuilder(PropertyInfo targetProperty)
                : base(targetProperty)
            { }

            internal override FieldDefinition<TClass> GetFieldDefinition()
            {
                return FieldDefinition<TClass>.Create(TargetProperty, Offset.GetValueOrDefault(), ByteOrder, null);
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

            public StringFieldBuilder IsInterned() => IsInterned(true);
            public StringFieldBuilder IsInterned(bool isInterned)
            {
                this.isInterned = isInterned;
                return this;
            }

            public StringFieldBuilder IsLengthPrefixed()
            {
                isNullTerminated = isFixedLength = false;
                isLengthPrefixed = true;
                return this;
            }

            public StringFieldBuilder IsFixedLength(int length, bool trim = false, char padding = ' ')
            {
                isNullTerminated = isLengthPrefixed = false;
                isFixedLength = true;
                this.length = length;
                trimEnabled = trim;
                paddingChar = padding;
                return this;
            }

            public StringFieldBuilder IsNullTerminated() => IsNullTerminated(default);

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
