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
        public void WriteComplex<T>(T value) where T : new()
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteComplexInternal(value, null, false);
        }

        public void WriteComplex<T>(T value, double version) where T : new()
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteComplexInternal(value, version, false);
        }

        public void WriteComplex(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteComplexInternal(value, null, false);
        }

        public void WriteComplex(object value, double version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteComplexInternal(value, version, false);
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

        private void WriteComplexInternal(object value, double? version, bool isProperty)
        {
            var type = value.GetType();
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw Exceptions.TypeNotConstructable(type.Name, isProperty);

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
                    writer.ByteOrder = attr.ByteOrder;
                }

                var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Utils.CheckPropertyForRead(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                    writer.WritePropertyValue(value, prop, version);
            }

            if (Attribute.IsDefined(type, typeof(ObjectSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ObjectSizeAttribute>(type, version);
                BaseStream.Position = originalPosition + attr.Size;
            }
        }

        private void WritePropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);

            if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                ByteOrder = attr.ByteOrder;
            }

            var value = prop.GetValue(obj);

            //in case this was called with a specific version number we should write that number
            //instead of [VersionNumber] property values to ensure the object can be read back in again
            if (Attribute.IsDefined(prop, typeof(VersionNumberAttribute)) && version.HasValue)
            {
                var converter = TypeDescriptor.GetConverter(typeof(double));
                if (converter.CanConvertTo(prop.PropertyType))
                    value = converter.ConvertTo(version.Value, prop.PropertyType);
            }

            if (prop.PropertyType.IsPrimitive)
                WritePrimitiveValue(value);
            else if (prop.PropertyType.Equals(typeof(string)))
                WriteStringValue(obj, prop);
            else if (prop.PropertyType.Equals(typeof(Guid)))
                Write((Guid)value);
            else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = prop.PropertyType.GetGenericArguments()[0];
                if (innerType.IsPrimitive)
                    WritePrimitiveValue(value ?? Activator.CreateInstance(innerType));
                else if (innerType.Equals(typeof(Guid)))
                    Write(value == null ? Guid.Empty : (Guid)value);
                else
                {
                    if (innerType.GetConstructor(Type.EmptyTypes) == null)
                        throw Exceptions.TypeNotConstructable(innerType.Name, true);

                    WriteComplexInternal(value ?? Activator.CreateInstance(innerType), version, true);
                }
            }
            else WriteComplexInternal(value, version, true);

            ByteOrder = originalByteOrder;
        }
    }
}
