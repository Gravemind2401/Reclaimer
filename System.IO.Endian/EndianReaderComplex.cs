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
        /// <param name="property">The property to check.</param>
        /// <param name="version">The version to use when checking the property.</param>
        protected virtual bool CanReadProperty(PropertyInfo property, double? version)
        {
            return Utils.CheckPropertyForReadWrite(property, version);
        }

        /// <summary>
        /// Reads a property from the current position in the stream and sets the property value
        /// against the specified instance of the property's containing type.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="property">The property to read.</param>
        /// <param name="version">The version to use when reading the property.</param>
        protected virtual void ReadProperty(object instance, PropertyInfo property, double? version)
        {
            if (property.GetGetMethod() == null || property.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(property.Name);

            if (Attribute.IsDefined(property, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(property, version);
                ByteOrder = attr.ByteOrder;
            }

            object value = null;
            var readType = property.PropertyType;

            if (Attribute.IsDefined(property, typeof(StoreTypeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<StoreTypeAttribute>(property, version);
                readType = attr.StoreType;
            }

            if (readType.IsEnum)
                readType = readType.GetEnumUnderlyingType();

            if (readType.IsPrimitive)
                value = ReadPrimitiveValue(readType);
            else if (readType.Equals(typeof(string)))
                value = ReadStringValue(property, version);
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

            var propType = property.PropertyType.IsEnum ? property.PropertyType.GetEnumUnderlyingType() : property.PropertyType;
            if (readType != propType)
            {
                var converter = TypeDescriptor.GetConverter(readType);
                if (converter.CanConvertTo(propType))
                    value = converter.ConvertTo(value, propType);
                else throw Exceptions.PropertyNotConvertable(property.Name, readType.Name, propType.Name);
            }

            if (property.PropertyType.IsEnum)
                value = Enum.ToObject(property.PropertyType, value);

            property.SetValue(instance, value);
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
            var primitiveMethod = (from m in GetType().GetMethods()
                                   where m.Name.StartsWith("Read")
                                   && m.Name.Length > 4 //exclude "Read()"
                                   && m.ReturnType.Equals(type)
                                   && m.GetParameters().Length == 0
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveReadMethod(type.Name);
            else return primitiveMethod.Invoke(this, null);
        }

        private string ReadStringValue(PropertyInfo prop, double? version)
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
                    reader.ByteOrder = attr.ByteOrder;
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
                BaseStream.Position = originalPosition + attr.Size;
            }

            return instance;
        }
    }
}
