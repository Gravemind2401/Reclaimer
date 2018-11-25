using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

            WriteObjectInternal(value, null);
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

            WriteObjectInternal(value, version);
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

            WriteObjectInternal(value, null);
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

            WriteObjectInternal(value, version);
        } 

        #endregion

        /// <summary>
        /// Checks if the specified property is writable.
        /// </summary>
        /// <param name="prop">The property to check.</param>
        /// <param name="version">The version to use when checking the property.</param>
        /// <returns></returns>
        protected virtual bool CanWriteProperty(PropertyInfo prop, double? version)
        {
            return Utils.CheckPropertyForReadWrite(prop, version);
        }

        /// <summary>
        /// Writes a property to the current position in the stream using the value of
        /// the property against the specified instance of the property's containing type.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="prop">The property to write.</param>
        /// <param name="version">The version to use when writing the property.</param>
        protected virtual void WriteProperty(object instance, PropertyInfo prop, double? version)
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

            var value = prop.GetValue(instance);
            var writeType = prop.PropertyType;

            if (Attribute.IsDefined(prop, typeof(StoreTypeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<StoreTypeAttribute>(prop, version);
                if (attr != null) writeType = attr.StoreType;
            }

            if (writeType.IsEnum)
                writeType = writeType.GetEnumUnderlyingType();

            //in case this was called with a specific version number we should write that number
            //instead of [VersionNumber] property values to ensure the object can be read back in again
            if (Attribute.IsDefined(prop, typeof(VersionNumberAttribute)) && version.HasValue)
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
                value = Convert.ChangeType(value, valType, CultureInfo.InvariantCulture);
            }

            if (writeType != valType)
            {
                var converter = TypeDescriptor.GetConverter(valType);
                if (converter.CanConvertTo(writeType))
                    value = converter.ConvertTo(value, writeType);
                else throw Exceptions.PropertyNotConvertable(prop.Name, writeType.Name, valType.Name);
            }

            if (writeType.IsPrimitive)
                WritePrimitiveValue(value);
            else if (writeType.Equals(typeof(string)))
                WriteStringValue(instance, prop);
            else if (writeType.Equals(typeof(Guid)))
                Write((Guid)value);
            else WriteObjectInternal(value, version);
        }

        private void WritePropertyValue(object instance, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
            WriteProperty(instance, prop, version);
            ByteOrder = originalByteOrder;
        }

        private void WritePrimitiveValue(object value)
        {
            var type = value.GetType();
            var primitiveMethod = (from m in typeof(EndianWriter).GetMethods()
                                   let param = m.GetParameters()
                                   where m.Name.Equals(nameof(Write))
                                   && param.Length == 1
                                   && param[0].ParameterType.Equals(type)
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveWriteMethod(type.Name);
            else primitiveMethod.Invoke(this, new[] { value });
        }

        private void WriteStringValue(object instance, PropertyInfo prop)
        {
            var value = (string)prop.GetValue(instance);

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

        private static double? GetVersionValue(object instance, Type type)
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
                }

                var converter = TypeDescriptor.GetConverter(vprop.PropertyType);
                if (converter.CanConvertTo(typeof(double)))
                    version = (double)converter.ConvertTo(vprop.GetValue(instance), typeof(double));
            }

            return version;
        }

        private void WriteObjectInternal(object value, double? version)
        {
            var type = value.GetType();
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            var originalPosition = BaseStream.Position;
            using (var writer = CreateVirtualWriter())
            {
                if (!version.HasValue)
                    version = GetVersionValue(value, type);

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
                        var len = (long)converter.ConvertTo(lengthProps[0].GetValue(value), typeof(long));
                        BaseStream.Position = originalPosition + len;
                    }
                }
            }

            if (Attribute.IsDefined(type, typeof(FixedSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<FixedSizeAttribute>(type, version);
                if (attr != null) BaseStream.Position = originalPosition + attr.Size;
            }
        }
    }
}
