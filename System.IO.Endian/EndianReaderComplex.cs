﻿using System;
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
                        var offsets = Utils.GetCustomAttributes<OffsetAttribute>(vprop).ToList();
                        if (offsets.Count > 1 || offsets[0].HasMinVersion || offsets[0].HasMaxVersion)
                            throw Exceptions.InvalidVersionAttribute();

                        reader.ReadProperty(result, vprop, null);
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
                    .Where(p => Utils.CheckPropertyForReadWrite(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                {
                    var originalByteOrder = reader.ByteOrder;
                    reader.Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);

                    reader.ReadProperty(result, prop, version);

                    reader.ByteOrder = originalByteOrder;
                }
            }

            if (Attribute.IsDefined(type, typeof(ObjectSizeAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ObjectSizeAttribute>(type, version);
                BaseStream.Position = originalPosition + attr.Size;
            }

            return result;
        }

        protected virtual void ReadProperty(object obj, PropertyInfo prop, double? version)
        {
            if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
                throw Exceptions.NonPublicGetSet(prop.Name);

            if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
            {
                var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                ByteOrder = attr.ByteOrder;
            }

            if (prop.PropertyType.IsPrimitive)
                prop.SetValue(obj, ReadPrimitiveValue(prop.PropertyType));
            else if (prop.PropertyType.Equals(typeof(string)))
                prop.SetValue(obj, ReadStringValue(prop, version));
            else if (prop.PropertyType.Equals(typeof(Guid)))
                prop.SetValue(obj, ReadGuid());
            else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                var innerType = prop.PropertyType.GetGenericArguments()[0];
                if (innerType.IsPrimitive)
                    prop.SetValue(obj, ReadPrimitiveValue(innerType));
                else if (innerType.Equals(typeof(Guid)))
                    prop.SetValue(obj, ReadGuid());
                else prop.SetValue(obj, ReadComplexInternal(innerType, version, true));
            }
            else prop.SetValue(obj, ReadComplexInternal(prop.PropertyType, version, true));
        }

        protected virtual bool CanReadProperty(PropertyInfo property, double? version)
        {
            return Utils.CheckPropertyForReadWrite(property, version);
        }
    }
}
