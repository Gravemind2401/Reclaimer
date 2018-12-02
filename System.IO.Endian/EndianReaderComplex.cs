using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    public partial class EndianReader : BinaryReader
    {
        #region ReadObject Overloads

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>()
        {
            return (T)ReadObject(null, typeof(T), null);
        }

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(double version)
        {
            return (T)ReadObject(null, typeof(T), version);
        }

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, null);
        }

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type, double version)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, version);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="instance">The object to populate.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(T instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), version);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(object instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return ReadObject(instance, instance.GetType(), version);
        }

        #endregion

        /// <summary>
        /// Checks if the specified property is readable.
        /// </summary>
        /// <param name="prop">The property to check.</param>
        /// <param name="version">The version to use when checking the property.</param>
        protected virtual bool CanReadProperty(PropertyInfo prop, double? version)
        {
            return Utils.CheckPropertyForReadWrite(prop, version);
        }

        /// <summary>
        /// This method is called for each property of an object when reading complex objects
        /// to read the property value from the stream. The <seealso cref="ByteOrder"/> and
        /// position of the stream are set before this method is called.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="prop">The property to read.</param>
        /// <param name="storeType">The type specified by this property's <seealso cref="StoreTypeAttribute"/>, if any.</param>
        /// <param name="version">The version to use when reading the property.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="MissingMethodException"/>
        protected virtual void ReadProperty(object instance, PropertyInfo prop, Type storeType, double? version)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            if (storeType == null)
                throw new ArgumentNullException(nameof(storeType));

            if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(prop.Name);

            object value = null;

            if (storeType.IsEnum)
                storeType = storeType.GetEnumUnderlyingType();

            if (storeType.IsPrimitive || storeType.Equals(typeof(Guid)))
                value = ReadStandardValue(storeType);
            else if (storeType.Equals(typeof(string)))
                value = ReadStringValue(prop);
            else if (storeType.IsGenericType && storeType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = storeType.GetGenericArguments()[0];
                if (innerType.IsPrimitive || innerType.Equals(typeof(Guid)))
                    value = ReadStandardValue(innerType);
                else value = ReadObject(null, innerType, version);
            }
            else value = ReadObject(null, storeType, version);

            var propType = prop.PropertyType.IsEnum ? prop.PropertyType.GetEnumUnderlyingType() : prop.PropertyType;
            if (storeType != propType)
            {
                var converter = TypeDescriptor.GetConverter(storeType);
                if (converter.CanConvertTo(propType))
                    value = converter.ConvertTo(value, propType);
                else throw Exceptions.PropertyNotConvertable(prop.Name, storeType.Name, propType.Name);
            }

            if (prop.PropertyType.IsEnum)
                value = Enum.ToObject(prop.PropertyType, value);

            prop.SetValue(instance, value);
        }

        private void ReadPropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            var storeType = prop.PropertyType;

            if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                if (attr != null) ByteOrder = attr.ByteOrder;
            }

            if (Attribute.IsDefined(prop, typeof(StoreTypeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<StoreTypeAttribute>(prop, version);
                if (attr != null) storeType = attr.StoreType;
            }

            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
            ReadProperty(obj, prop, storeType, version);

            ByteOrder = originalByteOrder;
        }

        /// <summary>
        /// Reads and returns a primitive type or <seealso cref="Guid"/> from the underlying stream.
        /// </summary>
        /// <param name="type">The primitive type to read, or <seealso cref="Guid"/>.</param>
        protected object ReadStandardValue(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var primitiveMethod = (from m in typeof(EndianReader).GetMethods()
                                   where m.Name.StartsWith(nameof(Read), StringComparison.Ordinal)
                                   && !m.Name.Equals(nameof(Read), StringComparison.Ordinal)
                                   && m.ReturnType.Equals(type)
                                   && m.GetParameters().Length == 0
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveReadMethod(type.Name);
            else return primitiveMethod.Invoke(this, null);
        }

        /// <summary>
        /// Reads a string value from the underlying stream. The method used to read the string
        /// is determined by the attributes applied to the supplied property.
        /// </summary>
        /// <param name="prop">The string property.</param>
        protected string ReadStringValue(PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            var lenPrefixed = Utils.GetCustomAttribute<LengthPrefixedAttribute>(prop);
            var fixedLen = Utils.GetCustomAttribute<FixedLengthAttribute>(prop);
            var nullTerm = Utils.GetCustomAttribute<NullTerminatedAttribute>(prop);

            string value = null;
            if (lenPrefixed == null && fixedLen == null && nullTerm == null)
                throw Exceptions.StringTypeUnknown(prop.Name);

            if (lenPrefixed != null)
                value = ReadString();

            if (fixedLen != null)
            {
                if (lenPrefixed != null)
                    throw Exceptions.StringTypeOverlap(prop.Name);

                value = ReadString(fixedLen.Length, fixedLen.Trim);
            }

            if (nullTerm != null)
            {
                if (lenPrefixed != null || fixedLen != null)
                    throw Exceptions.StringTypeOverlap(prop.Name);

                if (nullTerm.HasLength)
                    value = ReadNullTerminatedString(nullTerm.Length);
                else value = ReadNullTerminatedString();
            }

            return value;
        }

        private double? GetVersionValue(object instance, Type type)
        {
            double? version = null;

            var versionProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Attribute.IsDefined(p, typeof(VersionNumberAttribute)))
                .ToList();

            if (versionProps.Count > 1)
                throw Exceptions.MultipleVersionsSpecified(type.Name);
            else if (versionProps.Count == 1)
            {
                var vprop = versionProps[0];
                if (Attribute.IsDefined(vprop, typeof(OffsetAttribute)))
                {
                    var offsets = Utils.GetCustomAttributes<OffsetAttribute>(vprop).ToList();
                    if (offsets.Count > 1 || offsets[0].HasMinVersion || offsets[0].HasMaxVersion)
                        throw Exceptions.InvalidVersionAttribute();

                    ReadPropertyValue(instance, vprop, null);
                }

                var converter = TypeDescriptor.GetConverter(vprop.PropertyType);
                if (converter.CanConvertTo(typeof(double)))
                    version = (double)converter.ConvertTo(vprop.GetValue(instance), typeof(double));
            }

            return version;
        }

        /// <summary>
        /// This function is called by all public ReadObject overloads.
        /// </summary>
        /// <param name="instance">The object to populate. This value will be null if no instance was provided.</param>
        /// <param name="type">The type of object to read.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// This value will be null if no version was provided.
        /// </param>
        protected virtual object ReadObject(object instance, Type type, double? version)
        {
            return ReadObjectInternal(instance, type, version, false);
        }

        private object ReadObjectInternal(object instance, Type type, double? version, bool isProperty)
        {
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();
            else if (type.IsPrimitive || type.Equals(typeof(Guid)))
                return ReadStandardValue(type);

            if (instance == null)
            {
                if (type.IsClass && type.GetConstructor(Type.EmptyTypes) == null)
                    throw Exceptions.TypeNotConstructable(type.Name, isProperty);

                instance = Activator.CreateInstance(type);
            }

            var originalPosition = BaseStream.Position;
            using (var reader = CreateVirtualReader())
            {
                if (!version.HasValue)
                {
                    version = reader.GetVersionValue(instance, type);
                    reader.Seek(0, SeekOrigin.Begin);
                }

                if (Attribute.IsDefined(type, typeof(ByteOrderAttribute)))
                {
                    var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                    if (attr != null) reader.ByteOrder = attr.ByteOrder;
                }

                var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Utils.CheckPropertyForReadWrite(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                    reader.ReadPropertyValue(instance, prop, version);

                var lengthProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Attribute.IsDefined(p, typeof(DataLengthAttribute)))
                    .ToList();

                if (lengthProps.Count > 1)
                    throw Exceptions.MultipleDataLengthsSpecified(type.Name, version);
                else if (lengthProps.Count == 1 && Utils.GetAttributeForVersion<DataLengthAttribute>(lengthProps[0], version) != null)
                {
                    var converter = TypeDescriptor.GetConverter(lengthProps[0].PropertyType);
                    if (converter.CanConvertTo(typeof(long)))
                    {
                        var len = (long)converter.ConvertTo(lengthProps[0].GetValue(instance), typeof(long));
                        BaseStream.Position = originalPosition + len;
                    }
                }
            }

            if (Attribute.IsDefined(type, typeof(FixedSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<FixedSizeAttribute>(type, version);
                if (attr != null) BaseStream.Position = originalPosition + attr.Size;
            }

            return instance;
        }
    }
}
