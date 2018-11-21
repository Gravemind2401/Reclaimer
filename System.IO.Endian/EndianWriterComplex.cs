using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    public partial class EndianWriter : BinaryWriter
    {
        #region WriteObject Overloads

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless conustructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <param name="value">The object to write.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObjectInternal(value, null, false);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless conustructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject<T>(T value, double version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObjectInternal(value, version, false);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless conustructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObjectInternal(value, null, false);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless conustructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject(object value, double version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObjectInternal(value, version, false);
        } 

        #endregion

        /// <summary>
        /// Checks if the specified property is writable.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <param name="version">The version to use when checking the property.</param>
        /// <returns></returns>
        protected virtual bool CanWriteProperty(PropertyInfo property, double? version)
        {
            return Utils.CheckPropertyForReadWrite(property, version);
        }

        /// <summary>
        /// Writes a property to the current position in the stream using the value of
        /// the property against the specified instance of the property's containing type.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="property">The property to write.</param>
        /// <param name="version">The version to use when writing the property.</param>
        protected virtual void WriteProperty(object instance, PropertyInfo property, double? version)
        {
            if (property.GetGetMethod() == null || property.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(property.Name);

            if (Attribute.IsDefined(property, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(property, version);
                if (attr != null) ByteOrder = attr.ByteOrder;
            }

            var value = property.GetValue(instance);
            var writeType = property.PropertyType;

            if (Attribute.IsDefined(property, typeof(StoreTypeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<StoreTypeAttribute>(property, version);
                if (attr != null) writeType = attr.StoreType;
            }

            if (writeType.IsEnum)
                writeType = writeType.GetEnumUnderlyingType();

            //in case this was called with a specific version number we should write that number
            //instead of [VersionNumber] property values to ensure the object can be read back in again
            if (Attribute.IsDefined(property, typeof(VersionNumberAttribute)) && version.HasValue)
            {
                var converter = TypeDescriptor.GetConverter(typeof(double));
                if (converter.CanConvertTo(writeType))
                    value = converter.ConvertTo(version.Value, writeType);
            }

            if (writeType.IsGenericType && writeType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = writeType.GetGenericArguments()[0];
                if (value == null)
                {
                    if (innerType.IsPrimitive)
                        value = Activator.CreateInstance(innerType);
                    else if (innerType.Equals(typeof(Guid)))
                        value = Guid.Empty;
                    else
                    {
                        if (innerType.GetConstructor(Type.EmptyTypes) == null)
                            throw Exceptions.TypeNotConstructable(innerType.Name, true);

                        value = Activator.CreateInstance(innerType);
                    }
                }
                writeType = innerType;
            }

            var valType = value.GetType();
            if (valType.IsEnum)
            {
                valType = valType.GetEnumUnderlyingType();
                value = Convert.ChangeType(value, valType);
            }

            if (writeType != valType)
            {
                var converter = TypeDescriptor.GetConverter(valType);
                if (converter.CanConvertTo(writeType))
                    value = converter.ConvertTo(value, writeType);
                else throw Exceptions.PropertyNotConvertable(property.Name, writeType.Name, valType.Name);
            }

            if (writeType.IsPrimitive)
                WritePrimitiveValue(value);
            else if (writeType.Equals(typeof(string)))
                WriteStringValue(instance, property);
            else if (writeType.Equals(typeof(Guid)))
                Write((Guid)value);
            else WriteObjectInternal(value, version, true);
        }

        private void WritePropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
            WriteProperty(obj, prop, version);
            ByteOrder = originalByteOrder;
        }

        private void WritePrimitiveValue(object value)
        {
            var type = value.GetType();
            var primitiveMethod = (from m in GetType().GetMethods()
                                   let param = m.GetParameters()
                                   where m.Name.Equals(nameof(Write))
                                   && param.Length == 1
                                   && param[0].ParameterType.Equals(type)
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveReadMethod(type.Name);
            else primitiveMethod.Invoke(this, new[] { value });
        }

        private void WriteStringValue(object obj, PropertyInfo prop)
        {
            var value = (string)prop.GetValue(obj);

            var lenPrefixed = Utils.GetCustomAttribute<LengthPrefixedAttribute>(prop);
            var fixedLen = Utils.GetCustomAttribute<FixedLengthAttribute>(prop);
            var nullTerm = Utils.GetCustomAttribute<NullTerminatedAttribute>(prop);

            if (lenPrefixed == null && fixedLen == null && nullTerm == null)
                throw Exceptions.StringTypeUnknown(prop.Name);

            if (lenPrefixed != null)
                Write(value);

            if (fixedLen != null)
            {
                if (lenPrefixed != null)
                    throw Exceptions.StringTypeOverlap(prop.Name);

                WriteStringFixedLength(value, fixedLen.Length, fixedLen.Padding);
            }

            if (nullTerm != null)
            {
                if (lenPrefixed != null || fixedLen != null)
                    throw Exceptions.StringTypeOverlap(prop.Name);

                if (nullTerm.HasLength)
                    WriteStringFixedLength(value, nullTerm.Length, '\0');
                else WriteStringNullTerminated(value);
            }
        }

        private void WriteObjectInternal(object value, double? version, bool isProperty)
        {
            var type = value.GetType();
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            var originalPosition = BaseStream.Position;
            using (var writer = CreateVirtualWriter())
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

                        var converter = TypeDescriptor.GetConverter(vprop.PropertyType);
                        if (converter.CanConvertTo(typeof(double)))
                            version = (double)converter.ConvertTo(vprop.GetValue(value), typeof(double));
                    }
                }

                if (Attribute.IsDefined(type, typeof(ByteOrderAttribute)))
                {
                    var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                    if (attr != null) writer.ByteOrder = attr.ByteOrder;
                }

                var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Utils.CheckPropertyForReadWrite(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                    writer.WritePropertyValue(value, prop, version);
            }

            if (Attribute.IsDefined(type, typeof(ObjectSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ObjectSizeAttribute>(type, version);
                if (attr != null) BaseStream.Position = originalPosition + attr.Size;
            }
        }
    }
}
