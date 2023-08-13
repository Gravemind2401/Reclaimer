using System.Diagnostics;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class PropertyConfiguration
    {
        private readonly MethodInfo GetMethod;
        private readonly MethodInfo SetMethod;

        public PropertyInfo Property { get; }
        public Type PropertyType => Property.PropertyType;

        public bool IsVersionNumber { get; }
        public bool IsLengthPrefixed { get; }
        public bool IsInterned { get; }

        public MinVersionAttribute MinVersionAttribute { get; }
        public MaxVersionAttribute MaxVersionAttribute { get; }
        public VersionSpecificAttribute VersionSpecificAttribute { get; }
        public FixedLengthAttribute FixedLengthAttribute { get; }
        public NullTerminatedAttribute NullTerminatedAttribute { get; }
        public IReadOnlyList<DataLengthAttribute> DataLengthAttributes { get; }
        public IReadOnlyList<OffsetAttribute> OffsetAttributes { get; }
        public IReadOnlyList<ByteOrderAttribute> ByteOrderAttributes { get; }
        public IReadOnlyList<StoreTypeAttribute> StoreTypeAttributes { get; }

        public bool IsVersionBound => MinVersionAttribute != null || MaxVersionAttribute != null || VersionSpecificAttribute != null;

        public bool IsVersionNullable
        {
            get
            {
                return !IsVersionBound
                    && DataLengthAttributes.SupportsNullVersion()
                    && OffsetAttributes.SupportsNullVersion()
                    && ByteOrderAttributes.SupportsNullVersion()
                    && StoreTypeAttributes.SupportsNullVersion();
            }
        }

        public PropertyConfiguration(PropertyInfo prop)
        {
            Property = prop;
            GetMethod = prop.GetGetMethod();
            SetMethod = prop.GetSetMethod();
            IsVersionNumber = Attribute.IsDefined(prop, typeof(VersionNumberAttribute));
            IsLengthPrefixed = Attribute.IsDefined(prop, typeof(LengthPrefixedAttribute));
            IsInterned = Attribute.IsDefined(prop, typeof(InternedAttribute));
            MinVersionAttribute = prop.GetCustomAttribute<MinVersionAttribute>();
            MaxVersionAttribute = prop.GetCustomAttribute<MaxVersionAttribute>();
            VersionSpecificAttribute = prop.GetCustomAttribute<VersionSpecificAttribute>();
            FixedLengthAttribute = prop.GetCustomAttribute<FixedLengthAttribute>();
            NullTerminatedAttribute = prop.GetCustomAttribute<NullTerminatedAttribute>();
            DataLengthAttributes = prop.GetCustomAttributes<DataLengthAttribute>().ToList();
            OffsetAttributes = prop.GetCustomAttributes<OffsetAttribute>().ToList();
            ByteOrderAttributes = prop.GetCustomAttributes<ByteOrderAttribute>().ToList();
            StoreTypeAttributes = prop.GetCustomAttributes<StoreTypeAttribute>().ToList();
        }

        public object GetValue(object instance) => GetMethod.Invoke(instance, Type.EmptyTypes);
        public void SetValue(object instance, object value) => SetMethod.Invoke(instance, new object[] { value });

        public bool Validate()
        {
            if (OffsetAttributes.Count == 0)
                return false;

            if (GetMethod == null || SetMethod == null)
                throw Exceptions.NonPublicGetSet(Property);
            if (IsVersionNumber && !IsVersionNullable)
                throw new InvalidOperationException();
            if (!DataLengthAttributes.ValidateOverlap())
                throw Exceptions.AttributeVersionOverlap(Property, typeof(DataLengthAttribute));
            if (!OffsetAttributes.ValidateOverlap())
                throw Exceptions.AttributeVersionOverlap(Property, typeof(OffsetAttribute));
            if (!ByteOrderAttributes.ValidateOverlap())
                throw Exceptions.AttributeVersionOverlap(Property, typeof(ByteOrderAttribute));
            if (!StoreTypeAttributes.ValidateOverlap())
                throw Exceptions.AttributeVersionOverlap(Property, typeof(StoreTypeAttribute));

            var stringConstraints = Convert.ToInt32(IsLengthPrefixed) + Convert.ToInt32(FixedLengthAttribute != null) + Convert.ToInt32(NullTerminatedAttribute != null);
            if (PropertyType != typeof(string) && stringConstraints > 0)
                throw new InvalidOperationException("String type attributes applied to a non-string property");
            if (PropertyType == typeof(string) && stringConstraints != 1)
                throw Exceptions.StringTypeOverlap(Property);

            return true;
        }

        public bool ValidateVersion(double? version) => Extensions.ValidateVersion(version, MinVersionAttribute?.MinVersion, MaxVersionAttribute?.MaxVersion);

        private string GetDebuggerDisplay() => $"{Property.DeclaringType.Name}.{Property.Name}";
    }
}
