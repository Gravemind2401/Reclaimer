using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public partial class EndianReader : BinaryReader
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> stdMethodCache = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> versionPropCache = new ConcurrentDictionary<Type, PropertyInfo>();
        private static readonly ConcurrentDictionary<string, PropertyInfo> lengthPropCache = new ConcurrentDictionary<string, PropertyInfo>();
        private static readonly ConcurrentDictionary<string, ConstructorInfo> ctorCache = new ConcurrentDictionary<string, ConstructorInfo>();
        private static readonly ConcurrentDictionary<ConstructorInfo, Type[]> ctorParamCache = new ConcurrentDictionary<ConstructorInfo, Type[]>();

        public static void SetDebugMode(bool enabled) => DynamicReader.SetDebugMode(enabled);

        public bool DynamicReadEnabled { get; set; }

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
        public T ReadObject<T>() => (T)ReadObject(null, typeof(T), null);

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
        public T ReadObject<T>(double version) => (T)ReadObject(null, typeof(T), version);

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

        private object DynamicRead(object instance, Type type, double? version)
        {
            var originalPosition = BaseStream.Position;
            if (instance == null)
                instance = CreateInstance(type, version);

            var read = typeof(DynamicReader<>).MakeGenericType(type)
                .GetMethod("Read", BindingFlags.Static | BindingFlags.Public);

            return read.Invoke(null, new object[] { this, version, instance, originalPosition });
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

            object value;

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
                else
                    value = ReadObject(null, innerType, version);
            }
            else
                value = ReadObject(null, storeType, version);

            var propType = prop.PropertyType.IsEnum ? prop.PropertyType.GetEnumUnderlyingType() : prop.PropertyType;
            if (value != null && storeType != propType && !Utils.TryConvert(ref value, storeType, propType))
                throw Exceptions.PropertyNotConvertable(prop.Name, storeType.Name, propType.Name);

            if (prop.PropertyType.IsEnum)
                value = Enum.ToObject(prop.PropertyType, value);

            prop.SetValue(instance, value);
        }

        private void ReadPropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            var storeType = prop.PropertyType;

            var boAttr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
            if (boAttr != null)
                ByteOrder = boAttr.ByteOrder;

            var stAttr = Utils.GetAttributeForVersion<StoreTypeAttribute>(prop, version);
            if (stAttr != null)
                storeType = stAttr.StoreType;

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

            if (stdMethodCache.ContainsKey(type))
                return stdMethodCache[type].Invoke(this, null);

            var primitiveMethod = (from m in typeof(EndianReader).GetMethods()
                                   where m.Name.StartsWith(nameof(Read), StringComparison.Ordinal)
                                   && !m.Name.Equals(nameof(Read), StringComparison.Ordinal)
                                   && m.ReturnType.Equals(type)
                                   && m.GetParameters().Length == 0
                                   select m).SingleOrDefault();

            if (primitiveMethod == null)
                throw Exceptions.MissingPrimitiveReadMethod(type.Name);

            stdMethodCache.TryAdd(type, primitiveMethod);
            return primitiveMethod.Invoke(this, null);
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

                value = nullTerm.HasLength
                    ? ReadNullTerminatedString(nullTerm.Length)
                    : ReadNullTerminatedString();
            }

            return value;
        }

        private double? GetVersionValue(object instance, Type type)
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

                    ReadPropertyValue(instance, versionProp, null);
                }

                var temp = versionProp.GetValue(instance);

                var versionType = versionProp.PropertyType;
                if (versionType.IsEnum)
                {
                    versionType = versionType.GetEnumUnderlyingType();
                    temp = Convert.ChangeType(temp, versionType);
                }

                if (temp != null && Utils.TryConvert(ref temp, versionType, typeof(double)))
                    version = (double)temp;
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
            if (DynamicReadEnabled && !type.IsPrimitive)
                return DynamicRead(instance, type, version);
            else
                return ReadObjectInternal(instance, type, version);
        }

        protected virtual object CreateInstance(Type type, double? version)
        {
            var typeKey = Utils.CurrentCulture($"{type.FullName}:{version}");

            ConstructorInfo ctorInfo;
            if (ctorCache.ContainsKey(typeKey))
                ctorInfo = ctorCache[typeKey];
            else
            {
                var constructors = type.GetConstructors()
                    .Where(c => Utils.GetAttributeForVersion<BinaryConstructorAttribute>(c, version) != null);

                if (constructors.Count() > 1)
                    throw Exceptions.MultipleBinaryConstructorsSpecified(type.Name, version);

                ctorInfo = constructors.FirstOrDefault();
                if (ctorInfo == null && type.IsClass && type.GetConstructor(Type.EmptyTypes) == null)
                    throw Exceptions.TypeNotConstructable(type.Name);
                else
                    ctorCache.TryAdd(typeKey, ctorInfo);
            }

            if (ctorInfo != null)
                return ConstructObject(ctorInfo);
            else
                return Activator.CreateInstance(type);
        }

        private object ConstructObject(ConstructorInfo ctorInfo)
        {
            Type[] types;
            if (ctorParamCache.ContainsKey(ctorInfo))
                types = ctorParamCache[ctorInfo];
            else
            {
                types = ctorInfo.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray();

                if (types.Any(t => !t.IsPrimitive))
                    throw Exceptions.NonPrimitiveBinaryConstructorParameter(ctorInfo.DeclaringType.Name);

                ctorParamCache.TryAdd(ctorInfo, types);
            }

            var args = types.Select(t => ReadStandardValue(t)).ToArray();
            return ctorInfo.Invoke(args);
        }

        private object ReadObjectInternal(object instance, Type type, double? version)
        {
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();
            else if (type.IsPrimitive || type.Equals(typeof(Guid)))
                return ReadStandardValue(type);

            var typeKey = Utils.CurrentCulture($"{type.FullName}:{version}");
            var originalPosition = BaseStream.Position;

            if (instance == null)
                instance = CreateInstance(type, version);

            using (var reader = CreateVirtualReader())
            {
                if (!version.HasValue)
                    version = reader.GetVersionValue(instance, type);

                var boAttr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                if (boAttr != null)
                    reader.ByteOrder = boAttr.ByteOrder;

                var propInfo = Utils.GetProperties(type, version);
                foreach (var prop in propInfo)
                    reader.ReadPropertyValue(instance, prop, version);

                PropertyInfo lengthProp;
                if (lengthPropCache.ContainsKey(typeKey))
                    lengthProp = lengthPropCache[typeKey];
                else
                {
                    var lengthProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => Attribute.IsDefined(p, typeof(DataLengthAttribute)));

                    if (lengthProps.Count() > 1)
                        throw Exceptions.MultipleDataLengthsSpecified(type.Name, version);

                    lengthProp = lengthProps.FirstOrDefault();
                    lengthPropCache.TryAdd(typeKey, lengthProp);
                }

                if (lengthProp != null && Utils.GetAttributeForVersion<DataLengthAttribute>(lengthProp, version) != null)
                {
                    var temp = lengthProp.GetValue(instance);
                    if (temp != null && Utils.TryConvert(ref temp, lengthProp.PropertyType, typeof(long)))
                        SeekAbsolute(originalPosition + (long)temp);
                }
            }

            var fsAttr = Utils.GetAttributeForVersion<FixedSizeAttribute>(type, version);
            if (fsAttr != null)
                SeekAbsolute(originalPosition + fsAttr.Size);

            return instance;
        }
    }
}
