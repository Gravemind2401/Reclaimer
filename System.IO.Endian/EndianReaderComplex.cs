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
        public T ReadComplex<T>() where T : new()
        {
            return (T)ReadComplexInternal(typeof(T), null, false);
        }

        public T ReadComplex<T>(double version) where T : new()
        {
            return (T)ReadComplexInternal(typeof(T), version, false);
        }

        public object ReadComplex(Type type)
        {
            return ReadComplexInternal(type, null, false);
        }

        public object ReadComplex(Type type, double version)
        {
            return ReadComplexInternal(type, version, false);
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

        private bool ReadStringValue(PropertyInfo prop, out string value)
        {
            var lenPrefixed = (LengthPrefixedAttribute)Attribute.GetCustomAttribute(prop, typeof(LengthPrefixedAttribute));
            var fixedLen = (FixedLengthAttribute)Attribute.GetCustomAttribute(prop, typeof(FixedLengthAttribute));
            var nullTerm = (NullTerminatedAttribute)Attribute.GetCustomAttribute(prop, typeof(NullTerminatedAttribute));

            value = null;
            if (lenPrefixed == null && fixedLen == null && nullTerm == null)
                return false;

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

            return true;
        }

        private object ReadComplexInternal(Type type, double? version, bool isProperty)
        {
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw Exceptions.TypeNotConstructable(type.Name, isProperty);

            var result = Activator.CreateInstance(type);

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
                        var offsets = Attribute.GetCustomAttributes(vprop, typeof(OffsetAttribute)).OfType<OffsetAttribute>().ToList();
                        if (offsets.Count > 1 || offsets[0].HasMinVersion || offsets[0].HasMaxVersion)
                            throw Exceptions.InvalidVersionAttribute();

                        reader.ReadPropertyValue(result, vprop, null);
                        var converter = TypeDescriptor.GetConverter(vprop.PropertyType);
                        if (converter.CanConvertTo(typeof(double)))
                            version = (double)converter.ConvertTo(vprop.GetValue(result), typeof(double));

                        reader.Seek(0, SeekOrigin.Begin);
                    }
                }

                if (Attribute.IsDefined(type, typeof(ByteOrderAttribute)))
                {
                    var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                    reader.ByteOrder = attr.ByteOrder;
                }

                var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Utils.CheckPropertyForRead(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                    reader.ReadPropertyValue(result, prop, version);
            }

            if (Attribute.IsDefined(type, typeof(ObjectSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ObjectSizeAttribute>(type, version);
                BaseStream.Position = originalPosition + attr.Size;
            }

            return result;
        }

        private void ReadPropertyValue(object obj, PropertyInfo prop, double? version)
        {
            var originalByteOrder = ByteOrder;
            Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);

            if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                ByteOrder = attr.ByteOrder;
            }

            if (prop.PropertyType.IsPrimitive)
                prop.SetValue(obj, ReadPrimitiveValue(prop.PropertyType));
            else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = prop.PropertyType.GetGenericArguments()[0];
                if (innerType.IsPrimitive)
                    prop.SetValue(obj, ReadPrimitiveValue(innerType));
                else prop.SetValue(obj, ReadComplexInternal(innerType, version, true));
            }
            else if (prop.PropertyType.Equals(typeof(string)))
            {
                string value;
                if (ReadStringValue(prop, out value))
                    prop.SetValue(obj, value);
            }
            else prop.SetValue(obj, ReadComplexInternal(prop.PropertyType, version, true));

            ByteOrder = originalByteOrder;
        }
    }
}
