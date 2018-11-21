using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        public T ReadObject<T>() where T : new()
        {
            return (T)ReadObjectInternal(null, typeof(T), null, false);
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
        public T ReadObject<T>(double version) where T : new()
        {
            return (T)ReadObjectInternal(null, typeof(T), version, false);
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
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type)
        {
            return ReadObjectInternal(null, type, null, false);
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
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type, double version)
        {
            return ReadObjectInternal(null, type, version, false);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="instance">The object to populate.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void ReadObject<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ReadObjectInternal(instance, instance.GetType(), null, false);
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
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void ReadObject<T>(T instance, double version) where T : new()
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ReadObjectInternal(instance, instance.GetType(), version, false);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
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
        public void ReadObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ReadObjectInternal(instance, instance.GetType(), null, false);
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
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void ReadObject(object instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            ReadObjectInternal(instance, instance.GetType(), version, false);
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
        /// Reads a property from the current position in the stream and sets the property value
        /// against the specified instance of the property's containing type.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="prop">The property to read.</param>
        /// <param name="version">The version to use when reading the property.</param>
        protected virtual void ReadProperty(object instance, PropertyInfo prop, double? version)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(prop.Name);

            if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                if (attr != null) ByteOrder = attr.ByteOrder;
            }

            object value = null;
            var readType = prop.PropertyType;

            if (Attribute.IsDefined(prop, typeof(StoreTypeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<StoreTypeAttribute>(prop, version);
                if (attr != null) readType = attr.StoreType;
            }

            if (readType.IsEnum)
                readType = readType.GetEnumUnderlyingType();

            if (readType.IsPrimitive)
                value = ReadPrimitiveValue(readType);
            else if (readType.Equals(typeof(string)))
                value = ReadStringValue(prop);
            else if (readType.Equals(typeof(Guid)))
                value = ReadGuid();
            else if (readType.IsGenericType && readType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = readType.GetGenericArguments()[0];
                if (innerType.IsPrimitive)
                    value = ReadPrimitiveValue(innerType);
                else if (innerType.Equals(typeof(Guid)))
                    value = ReadGuid();
                else value = ReadObjectInternal(null, innerType, version, true);
            }
            else value = ReadObjectInternal(null, readType, version, true);

            var propType = prop.PropertyType.IsEnum ? prop.PropertyType.GetEnumUnderlyingType() : prop.PropertyType;
            if (readType != propType)
            {
                var converter = TypeDescriptor.GetConverter(readType);
                if (converter.CanConvertTo(propType))
                    value = converter.ConvertTo(value, propType);
                else throw Exceptions.PropertyNotConvertable(prop.Name, readType.Name, propType.Name);
            }

            if (prop.PropertyType.IsEnum)
                value = Enum.ToObject(prop.PropertyType, value);

            prop.SetValue(instance, value);
        }

        private void ReadPropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
            ReadProperty(obj, prop, null);
            ByteOrder = originalByteOrder;
        }

        private object ReadPrimitiveValue(Type type)
        {
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

        private string ReadStringValue(PropertyInfo prop)
        {
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

        private object ReadObjectInternal(object instance, Type type, double? version, bool isProperty)
        {
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            if (instance == null)
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    throw Exceptions.TypeNotConstructable(type.Name, isProperty);

                instance = Activator.CreateInstance(type);
            }

            var originalPosition = BaseStream.Position;
            using (var reader = CreateVirtualReader())
            {
                if (!version.HasValue)
                {
                    var versionProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => Attribute.IsDefined(p, typeof(OffsetAttribute)) && Attribute.IsDefined(p, typeof(VersionNumberAttribute)))
                        .ToList();

                    if (versionProps.Any())
                    {
                        if (versionProps.Count > 1)
                            throw Exceptions.MultipleVersionsSpecified(type.Name);

                        var vprop = versionProps[0];
                        var offsets = Utils.GetCustomAttributes<OffsetAttribute>(vprop).ToList();
                        if (offsets.Count > 1 || offsets[0].HasMinVersion || offsets[0].HasMaxVersion)
                            throw Exceptions.InvalidVersionAttribute();

                        reader.ReadPropertyValue(instance, vprop, null);
                        var converter = TypeDescriptor.GetConverter(vprop.PropertyType);
                        if (converter.CanConvertTo(typeof(double)))
                            version = (double)converter.ConvertTo(vprop.GetValue(instance), typeof(double));

                        reader.Seek(0, SeekOrigin.Begin);
                    }
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
            }

            if (Attribute.IsDefined(type, typeof(ObjectSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ObjectSizeAttribute>(type, version);
                if (attr != null) BaseStream.Position = originalPosition + attr.Size;
            }

            return instance;
        }
    }
}
