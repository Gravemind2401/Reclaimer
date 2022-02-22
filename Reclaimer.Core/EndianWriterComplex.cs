using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    public partial class EndianWriter : BinaryWriter
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> stdMethodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> versionPropCache = new ConcurrentDictionary<Type, PropertyInfo>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> lengthPropCache = new ConcurrentDictionary<Type, PropertyInfo>();

        #region WriteObject Overloads

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless constructor.
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

            WriteObject(value, null);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless constructor.
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

            WriteObject(value, (double?)version);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless constructor.
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

            WriteObject(value, null);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// The type being written must have a public parameterless constructor.
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

            WriteObject(value, (double?)version);
        }

        #endregion

        /// <summary>
        /// This method is called for each property of an object when writing complex objects
        /// to write the property value to the stream. The <seealso cref="ByteOrder"/> and
        /// position of the stream are set before this method is called.
        /// </summary>
        /// <param name="instance">The instance of the type containing the property.</param>
        /// <param name="prop">The property to write.</param>
        /// <param name="storeType">The type specified by this property's <seealso cref="StoreTypeAttribute"/>, if any.</param>
        /// <param name="version">The version to use when writing the property.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="MissingMethodException"/>
        protected virtual void WriteProperty(object instance, PropertyInfo prop, Type storeType, double? version)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            if (storeType == null)
                throw new ArgumentNullException(nameof(storeType));

            if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(prop.Name);

            var value = prop.GetValue(instance);

            if (storeType.IsEnum)
                storeType = storeType.GetEnumUnderlyingType();

            //in case this was called with a specific version number we should write that number
            //instead of [VersionNumber] property values to ensure the object can be read back in again
            if (Attribute.IsDefined(prop, typeof(VersionNumberAttribute)) && version.HasValue)
            {
                object temp = version.Value;
                if (temp != null && Utils.TryConvert(ref temp, typeof(double), storeType))
                    value = temp;
            }

            if (storeType.IsGenericType && storeType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = storeType.GetGenericArguments()[0];
                if (value == null)
                {
                    if (innerType.IsPrimitive)
                        value = Activator.CreateInstance(innerType);
                    else if (innerType.Equals(typeof(Guid)))
                        value = Guid.Empty;
                    else //inner type of Nullable<> will always be a struct so this should never fail
                        value = Activator.CreateInstance(innerType);
                }
                storeType = innerType;
            }

            var valType = value.GetType();
            if (valType.IsEnum)
            {
                valType = valType.GetEnumUnderlyingType();
                value = Convert.ChangeType(value, valType, CultureInfo.InvariantCulture);
            }

            if (value != null && storeType != valType && !Utils.TryConvert(ref value, valType, storeType))
                throw Exceptions.PropertyNotConvertable(prop.Name, storeType.Name, valType.Name);

            if (storeType.IsPrimitive || storeType.Equals(typeof(Guid)))
                WriteStandardValue(value);
            else if (storeType.Equals(typeof(string)))
                WriteStringValue(instance, prop);
            else WriteObject(value, version);
        }

        private void WritePropertyValue(object instance, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            var storeType = prop.PropertyType;

            var boAttr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
            if (boAttr != null) ByteOrder = boAttr.ByteOrder;

            var stAttr = Utils.GetAttributeForVersion<StoreTypeAttribute>(prop, version);
            if (stAttr != null) storeType = stAttr.StoreType;

            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
            WriteProperty(instance, prop, storeType, version);

            ByteOrder = originalByteOrder;
        }

        /// <summary>
        /// Writes a primitive type or <seealso cref="Guid"/> to the underlying stream.
        /// </summary>
        /// <param name="value">The instance of a primitive type or <seealso cref="Guid"/>.</param>
        protected void WriteStandardValue(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = value.GetType();
            if (stdMethodCache.ContainsKey(type))
            {
                stdMethodCache[type].Invoke(this, new[] { value });
                return;
            }

            var primitiveMethod = (from m in typeof(EndianWriter).GetMethods()
                                   let param = m.GetParameters()
                                   where m.Name.Equals(nameof(Write))
                                   && param.Length == 1
                                   && param[0].ParameterType.Equals(type)
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveWriteMethod(type.Name);

            stdMethodCache.TryAdd(type, primitiveMethod);
            primitiveMethod.Invoke(this, new[] { value });
        }

        /// <summary>
        /// Writes a string value to the underlying stream. The method used to write the string
        /// is determined by the attributes applied to the supplied property.
        /// </summary>
        /// <param name="instance">The instance of the object containing the string property.</param>
        /// <param name="prop">The string property.</param>
        protected void WriteStringValue(object instance, PropertyInfo prop)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

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

            PropertyInfo versionProp;
            if (versionPropCache.ContainsKey(type))
                versionProp = versionPropCache[type];
            else
            {
                var versionProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Attribute.IsDefined(p, typeof(VersionNumberAttribute)))
                .ToList();

                if (versionProps.Count > 1)
                    throw Exceptions.MultipleVersionsSpecified(type.Name);

                versionProp = versionProps.FirstOrDefault();
                versionPropCache.TryAdd(type, versionProp);
            }

            if (versionProp != null)
            {
                if (Attribute.IsDefined(versionProp, typeof(OffsetAttribute)))
                {
                    var offsets = Utils.GetCustomAttributes<OffsetAttribute>(versionProp).ToList();
                    if (offsets.Count > 1 || offsets[0].HasMinVersion || offsets[0].HasMaxVersion)
                        throw Exceptions.InvalidVersionAttribute();
                }

                var temp = versionProp.GetValue(instance);
                if (temp != null && Utils.TryConvert(ref temp, versionProp.PropertyType, typeof(double)))
                    version = (double)temp;
            }

            return version;
        }

        /// <summary>
        /// This function is called by all public WriteObject overloads.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </param>
        protected virtual void WriteObject(object value, double? version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObjectInternal(value, version);
        }

        private void WriteObjectInternal(object value, double? version)
        {
            var type = value.GetType();
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();
            else if (type.IsPrimitive || type.Equals(typeof(Guid)))
            {
                WriteStandardValue(value);
                return;
            }

            var originalPosition = BaseStream.Position;
            using (var writer = CreateVirtualWriter())
            {
                if (!version.HasValue)
                    version = GetVersionValue(value, type);

                var boAttr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                if (boAttr != null) writer.ByteOrder = boAttr.ByteOrder;

                var propInfo = Utils.GetProperties(type, version);
                foreach (var prop in propInfo)
                    writer.WritePropertyValue(value, prop, version);

                PropertyInfo lengthProp;
                if (lengthPropCache.ContainsKey(type))
                    lengthProp = lengthPropCache[type];
                else
                {
                    var lengthProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Attribute.IsDefined(p, typeof(DataLengthAttribute)));

                    if (lengthProps.Count() > 1)
                        throw Exceptions.MultipleDataLengthsSpecified(type.Name, version);

                    lengthProp = lengthProps.FirstOrDefault();
                    lengthPropCache.TryAdd(type, lengthProp);
                }

                if (lengthProp != null && Utils.GetAttributeForVersion<DataLengthAttribute>(lengthProp, version) != null)
                {
                    var temp = lengthProp.GetValue(value);
                    if (temp != null && Utils.TryConvert(ref temp, lengthProp.PropertyType, typeof(long)))
                        SeekAbsolute(originalPosition + (long)temp);
                }
            }

            var fsAttr = Utils.GetAttributeForVersion<FixedSizeAttribute>(type, version);
            if (fsAttr != null) SeekAbsolute(originalPosition + fsAttr.Size);
        }
    }
}
