using System;
using System.Collections.Generic;
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

        private MethodInfo GetPrimitiveReadMethod(Type type)
        {
            return (from m in GetType().GetMethods()
                    where m.Name.StartsWith("Read")
                    && m.Name.Length > 4 //exclude "Read()"
                    && m.ReturnType.Equals(type)
                    && m.GetParameters().Length == 0
                    select m).SingleOrDefault();
        }

        private bool ReadStringProperty(PropertyInfo prop, out string value)
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

                if (nullTerm.Length.HasValue)
                    value = ReadNullTerminatedString(nullTerm.Length.Value);
                else value = ReadNullTerminatedString();
            }

            return true;
        }

        private object ReadComplexInternal(Type type, double? version, bool isProperty)
        {
            if (type.IsPrimitive || type.Equals(typeof(string)))
                throw Exceptions.NotValidForPrimitiveTypes();

            var originalPosition = BaseStream.Position;

            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw Exceptions.TypeNotConstructable(type.Name, isProperty);

            var result = Activator.CreateInstance(type);

            using (var reader = CreateVirtualReader())
            {
                if (Attribute.IsDefined(type, typeof(ByteOrderAttribute)))
                {
                    var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(type, version);
                    reader.ByteOrder = attr.ByteOrder;
                }

                var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => Utils.CheckPropertyForRead(p, version))
                    .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset);

                foreach (var prop in propInfo)
                {
                    var originalByteOrder = reader.ByteOrder;
                    reader.Seek(Utils.GetAttributeForVersion<OffsetAttribute>(prop, version).Offset, SeekOrigin.Begin);
                                        
                    if (Attribute.IsDefined(prop, typeof(ByteOrderAttribute)))
                    {
                        var attr = Utils.GetAttributeForVersion<ByteOrderAttribute>(prop, version);
                        reader.ByteOrder = attr.ByteOrder;
                    }

                    if (prop.PropertyType.IsPrimitive)
                    {
                        var primitiveMethod = GetPrimitiveReadMethod(prop.PropertyType);
                        if (primitiveMethod == null)
                            throw Exceptions.MissingPrimitiveReadMethod(prop.PropertyType.Name);
                        prop.SetValue(result, primitiveMethod.Invoke(reader, null));
                    }
                    else if (prop.PropertyType.Equals(typeof(string)))
                    {
                        string value;
                        if (reader.ReadStringProperty(prop, out value))
                            prop.SetValue(result, value);
                    }
                    else prop.SetValue(result, reader.ReadComplexInternal(prop.PropertyType, version, true));

                    //if this property is the version number, use its value as the version for all following properties
                    if (Attribute.IsDefined(prop, typeof(VersionNumberAttribute)))
                        version = prop.GetValue(result) as double?;

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
    }
}
