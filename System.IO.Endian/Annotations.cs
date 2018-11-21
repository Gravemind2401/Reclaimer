using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    internal interface IVersionAttribute
    {
        double MinVersion { get; }
        double MaxVersion { get; }
        bool HasMinVersion { get; }
        bool HasMaxVersion { get; }
    }

    /// <summary>
    /// Specifies the size of an object when it is stored in a stream.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public sealed class ObjectSizeAttribute : Attribute, IVersionAttribute
    {
        private readonly long size;
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the size of the object, in bytes.
        /// </summary>
        public long Size => size;

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
            get { return minVersion.GetValueOrDefault(); }
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
            get { return maxVersion.GetValueOrDefault(); }
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ObjectSizeAttribute"/> class with the specified size value.
        /// </summary>
        /// <param name="size">The size of the object, in bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ObjectSizeAttribute(long size)
        {
            if (size <= 0)
                throw Exceptions.ParamMustBePositive(nameof(size), size);

            this.size = size;
        }
    }

    /// <summary>
    /// Specifies the offset of a property in a stream relative to the beginning of its containing type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class OffsetAttribute : Attribute, IVersionAttribute
    {
        private readonly long offset;
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the offset of the property's value, in bytes.
        /// </summary>
        public long Offset => offset;

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
            get { return minVersion.GetValueOrDefault(); }
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
            get { return maxVersion.GetValueOrDefault(); }
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
                throw Exceptions.ParamMustBeNonNegative(nameof(offset), offset);

            this.offset = offset;
        }
    }

    /// <summary>
    /// Specifies that a property holds the version number of the object being read or written.
    /// This attribute is only valid for integer and floating-point typed properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class VersionNumberAttribute : Attribute
    {

    }

    /// <summary>
    /// Specifies that a property is only applicable to a given range of versions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class VersionSpecificAttribute : Attribute
    {
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets a value indicating whether the property has a minimum version requirement.
        /// </summary>
        public bool HasMinVersion => minVersion.HasValue;

        /// <summary>
        /// Gets a value indicating whether the property has a maximum version requirement.
        /// </summary>
        public bool HasMaxVersion => maxVersion.HasValue;

        /// <summary>
        /// Gets or sets the inclusive minimum version that the property is applicable to.
        /// </summary>
        public double MinVersion
        {
            get { return minVersion.GetValueOrDefault(); }
            set
            {
                if (value > maxVersion)
                    throw Exceptions.BoundaryOverlapMinimum(nameof(MinVersion), nameof(MaxVersion));

                minVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets the exclusive maximum version that the property is applicable to.
        /// </summary>
        public double MaxVersion
        {
            get { return maxVersion.GetValueOrDefault(); }
            set
            {
                if (value < minVersion)
                    throw Exceptions.BoundaryOverlapMaximum(nameof(MinVersion), nameof(MaxVersion));

                maxVersion = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="VersionSpecificAttribute"/> class with the specified minimum and maximum version values.
        /// </summary>
        /// <param name="minVersion">The inclusive minimum version that the property applies to.</param>
        /// <param name="maxVersion">The exclusive maximum version that the property applies to.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public VersionSpecificAttribute(double minVersion, double maxVersion)
        {
            if (minVersion > maxVersion)
                throw Exceptions.BoundaryOverlapAmbiguous(nameof(minVersion), nameof(maxVersion));

            this.minVersion = minVersion;
            this.maxVersion = maxVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <seealso cref="VersionSpecificAttribute"/> class with the specified version value.
        /// </summary>
        /// <param name="version">The version that the property applies to.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public VersionSpecificAttribute(double version)
        {
            minVersion = version;
            maxVersion = version;
        }
    }

    /// <summary>
    /// Specifies that an object is stored using a specific byte order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class ByteOrderAttribute : Attribute, IVersionAttribute
    {
        private readonly ByteOrder byteOrder;
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the byte order used to store the object.
        /// </summary>
        public ByteOrder ByteOrder => byteOrder;

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
            get { return minVersion.GetValueOrDefault(); }
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
            get { return maxVersion.GetValueOrDefault(); }
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
            this.byteOrder = byteOrder;
        }
    }

    /// <summary>
    /// Specifies that a string property is stored as fixed-length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class FixedLengthAttribute : Attribute
    {
        private readonly int length;

        /// <summary>
        /// Gets the number of bytes used to store the string.
        /// </summary>
        public int Length => length;

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
                throw Exceptions.ParamMustBePositive(nameof(length), length);

            this.length = length;
            Padding = ' ';
        }
    }

    /// <summary>
    /// Specifies that a string is stored as null-terminated, optionally using a minimum number of bytes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class NullTerminatedAttribute : Attribute
    {
        private int? length;

        public bool HasLength => length.HasValue;

        /// <summary>
        /// Gets or sets the number of bytes used to store the string.
        /// </summary>
        public int Length
        {
            get { return length.GetValueOrDefault(); }
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
    public sealed class LengthPrefixedAttribute : Attribute
    {

    }

    /// <summary>
    /// Specifies that a property value should be converted when being read or written.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class StoreTypeAttribute : Attribute, IVersionAttribute
    {
        private readonly Type storeType;
        private double? minVersion;
        private double? maxVersion;

        /// <summary>
        /// Gets the byte order that used to store the object.
        /// </summary>
        public Type StoreType => storeType;

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
            get { return minVersion.GetValueOrDefault(); }
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
            get { return maxVersion.GetValueOrDefault(); }
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
            this.storeType = storeType;
        }
    }
}
