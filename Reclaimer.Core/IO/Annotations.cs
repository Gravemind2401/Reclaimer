using Reclaimer.IO.Dynamic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Reclaimer.IO
{
    internal interface IVersionAttribute
    {
        double MinVersion { get; }
        double MaxVersion { get; }
        bool HasMinVersion { get; }
        bool HasMaxVersion { get; }

        sealed bool IsVersioned => HasMinVersion || HasMaxVersion;
    }

    internal interface IStringTypeAttribute
    {
        //empty interface for easy OfType<> checks
    }

    /// <summary>
    /// Specifies the size, in bytes, of an object when it is stored in a stream. Overrides the <seealso cref="DataLengthAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class FixedSizeAttribute : Attribute, IVersionAttribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the size of the object, in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets a value indicating whether the size has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the size has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the size is applicable to.
        /// </summary>
        public double MinVersion
        {
            get => minVersion.GetValueOrDefault();
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the size is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get => maxVersion.GetValueOrDefault();
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="FixedSizeAttribute"/> class with the specified size value.
        /// </summary>
        /// <param name="size">The size of the object, in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public FixedSizeAttribute(long size)
        {
            Exceptions.ThrowIfNotPositive(size);
            Size = size;
        }

        /// <summary>
        /// Gets the value of the <see cref="FixedSizeAttribute"/> for a particular type, with no specific version.
        /// </summary>
        /// <param name="type">The type to check.</param>
        public static long ValueFor(Type type) => ValueFor(type, null);

        /// <summary>
        /// Gets the value of the <see cref="FixedSizeAttribute"/> for a particular type, given a particular version.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="version">The version to check.</param>
        public static long ValueFor(Type type, double? version)
        {
            return type.GetCustomAttributes<FixedSizeAttribute>().GetVersion(version).Size;
        }
    }

    /// <summary>
    /// Specifies that the property value represents the number of bytes used to store its containing instance.
    /// <br/>This attribute is only valid for integer typed properties. The property value must be a positive integer less than <see cref="long.MaxValue"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class DataLengthAttribute : Attribute, IVersionAttribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets a value indicating whether the length has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the length has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the length is applicable to.
        /// </summary>
        public double MinVersion
        {
            get => minVersion.GetValueOrDefault();
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the length is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get => maxVersion.GetValueOrDefault();
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        internal static void ThrowIfInvalidPropertyType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            if (typeCode < TypeCode.SByte || typeCode > TypeCode.UInt64)
                throw new InvalidDataException($"{nameof(DataLengthAttribute)} is not valid for properties stored as {type.Name}.");
        }
    }

    /// <summary>
    /// Specifies the offset of a property in a stream relative to the beginning of its containing type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class OffsetAttribute : Attribute, IVersionAttribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the offset of the property's value, in bytes.
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// Gets a value indicating whether the offset has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the offset has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the offset is applicable to.
        /// </summary>
        public double MinVersion
        {
            get => minVersion.GetValueOrDefault();
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the offset is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get => maxVersion.GetValueOrDefault();
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="OffsetAttribute"/> class with the specified offset value.
        /// </summary>
        /// <param name="offset">The offset of the property's value, in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public OffsetAttribute(long offset)
        {
            if (offset < 0)
                throw Exceptions.ParamMustBeNonNegative(offset);

            Offset = offset;
        }

        /// <summary>
        /// Gets the value of the <see cref="OffsetAttribute"/> for a particular property, with no specific version.
        /// </summary>
        /// <param name="expression">An expression referencing the property to check.</param>
        public static long ValueFor<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression) => ValueFor(Utils.PropertyFromExpression(expression), null);

        /// <summary>
        /// Gets the value of the <see cref="OffsetAttribute"/> for a particular property, given a particular version.
        /// </summary>
        /// <param name="expression">An expression referencing the property to check.</param>
        /// <param name="version">The version to check.</param>
        public static long ValueFor<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression, double? version) => ValueFor(Utils.PropertyFromExpression(expression), version);

        /// <summary>
        /// Gets the value of the <see cref="OffsetAttribute"/> for a particular property, with no specific version.
        /// </summary>
        /// <param name="prop">The property to check.</param>
        public static long ValueFor(PropertyInfo prop) => ValueFor(prop, null);

        /// <summary>
        /// Gets the value of the <see cref="OffsetAttribute"/> for a particular property, given a particular version.
        /// </summary>
        /// <param name="prop">The property to check.</param>
        /// <param name="version">The version to check.</param>
        public static long ValueFor(PropertyInfo prop, double? version)
        {
            return prop.GetCustomAttributes<OffsetAttribute>()
                .GetVersion(version).Offset;
        }
    }

    /// <summary>
    /// Specifies that a property holds the version number of the object being read or written.
    /// This attribute is only valid for integer and floating-point typed properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class VersionNumberAttribute : Attribute
    {
        internal static void ThrowIfInvalidPropertyType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            if (typeCode < TypeCode.SByte || typeCode > TypeCode.Double)
                throw new InvalidDataException($"{nameof(VersionNumberAttribute)} is not valid for properties stored as {type.Name}.");
        }
    }

    /// <summary>
    /// Specifies that a property is only applicable after a certain version.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class MinVersionAttribute : Attribute
    {
        /// <summary>
        /// Gets the inclusive minimum version that the property is applicable to.
        /// </summary>
        public double MinVersion { get; }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="MinVersionAttribute"/> class with the specified minimum version value.
        /// </summary>
        /// <param name="minVersion">The inclusive minimum version that the property is applicable to.</param>
        public MinVersionAttribute(double minVersion)
        {
            MinVersion = minVersion;
        }
    }

    /// <summary>
    /// Specifies that a property is only applicable until a certain version.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class MaxVersionAttribute : Attribute
    {
        /// <summary>
        /// Gets the exclusive maximum version that the property is applicable to.
        /// </summary>
        public double MaxVersion { get; }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="MaxVersionAttribute"/> class with the specified maximum version value.
        /// </summary>
        /// <param name="maxVersion">The exclusive maximum version that the property is applicable to.</param>
        public MaxVersionAttribute(double maxVersion)
        {
            MaxVersion = maxVersion;
        }
    }

    /// <summary>
    /// Specifies that a property is only applicable for a certain version.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class VersionSpecificAttribute : Attribute
    {
        /// <summary>
        /// Gets the version that the property is applicable to.
        /// </summary>
        public double Version { get; }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="VersionSpecificAttribute"/> class with the specified version value.
        /// </summary>
        /// <param name="version">The version that the property is applicable to.</param>
        public VersionSpecificAttribute(double version)
        {
            Version = version;
        }
    }

    /// <summary>
    /// Specifies that an object is stored using a specific byte order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class ByteOrderAttribute : Attribute, IVersionAttribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the byte order used to store the object.
        /// </summary>
        public ByteOrder ByteOrder { get; }

        /// <summary>
        /// Gets a value indicating whether the byte order has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the byte order has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the byte order is applicable to.
        /// </summary>
        public double MinVersion
        {
            get => minVersion.GetValueOrDefault();
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the byte order is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get => maxVersion.GetValueOrDefault();
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ByteOrderAttribute"/> class with the specified byte order value.
        /// </summary>
        /// <param name="byteOrder">The byte order that used to store the object.</param>
        public ByteOrderAttribute(ByteOrder byteOrder)
        {
            ByteOrder = byteOrder;
        }
    }

    /// <summary>
    /// Specifies that a string is stored as fixed-length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class FixedLengthAttribute : Attribute, IStringTypeAttribute
    {
        /// <summary>
        /// Gets the number of bytes used to store the string.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets or sets a value indicating if trailing white-space should be trimmed from the string.
        /// This value is only used by <seealso cref="EndianReader"/>.
        /// </summary>
        public bool Trim { get; set; }

        /// <summary>
        /// Gets or sets the character used as padding if the string is shorter than its assigned length.
        /// This value is only used by <seealso cref="EndianWriter"/>.
        /// </summary>
        public char Padding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="FixedLengthAttribute"/> class with the specified byte length value
        /// and space (0x20) as the padding character.
        /// </summary>
        /// <param name="length">The number of bytes used to store the string.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public FixedLengthAttribute(int length)
        {
            if (length <= 0)
                throw Exceptions.ParamMustBePositive(length);

            Length = length;
            Padding = ' ';
        }
    }

    /// <summary>
    /// Specifies that a string is stored as null-terminated, optionally using a minimum number of bytes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class NullTerminatedAttribute : Attribute, IStringTypeAttribute
    {
        private int? length;

        public bool HasLength => length.HasValue;

        /// <summary>
        /// Gets or sets the number of bytes used to store the string.
        /// </summary>
        public int Length
        {
            get => length.GetValueOrDefault();
            set
            {
                if (value < 0)
                    throw Exceptions.PropertyMustBeNullOrPositive(nameof(Length), length);

                length = value;
            }
        }
    }

    /// <summary>
    /// Specifies that a string is stored as length-prefixed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class LengthPrefixedAttribute : Attribute, IStringTypeAttribute
    {

    }

    /// <summary>
    /// Specifies that <see cref="string.Intern(string)"/> should be used when reading a string value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class InternedAttribute : Attribute
    {

    }

    /// <summary>
    /// Specifies that a property value should be converted when being read or written.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class StoreTypeAttribute : Attribute, IVersionAttribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the type that used to store the object.
        /// </summary>
        public Type StoreType { get; }

        /// <summary>
        /// Gets a value indicating whether the store type has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the store type has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the store type is applicable to.
        /// </summary>
        public double MinVersion
        {
            get => minVersion.GetValueOrDefault();
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the store type is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get => maxVersion.GetValueOrDefault();
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="StoreTypeAttribute"/> class 
        /// with the specified store type value.
        /// </summary>
        /// <param name="storeType"></param>
        public StoreTypeAttribute(Type storeType)
        {
            StoreType = storeType;
        }
    }
}
