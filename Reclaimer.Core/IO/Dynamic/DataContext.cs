using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO.Dynamic
{
    internal class DataContext
    {
        public object Target { get; }
        public long Origin { get; }
        public EndianReader Reader { get; }
        public EndianWriter Writer { get; }
        public ByteOrder ByteOrder { get; }
        public double? Version { get; set; }
        public long? DataLength { get; set; }

        public DataContext(TypeConfiguration manager, object instance, double? version, EndianReader reader)
        {
            Target = instance;
            Origin = reader.BaseStream.Position;
            Reader = reader;
            Version = version;
            ByteOrder = manager.ByteOrderAttributes.FirstOrDefault(ValidateVersion)?.ByteOrder ?? Reader.ByteOrder;
        }

        public bool ValidateVersion(PropertyConfiguration property) => ValidateVersion(Version, property.MinVersionAttribute?.MinVersion, property.MaxVersionAttribute?.MaxVersion);
        public bool ValidateVersion(IVersionAttribute attr) => ValidateVersion(Version, attr.HasMinVersion ? attr.MinVersion : null, attr.HasMaxVersion ? attr.MaxVersion : null);
        private static bool ValidateVersion(double? version, double? min, double? max)
        {
            return (version >= min || !min.HasValue) && (version < max || !max.HasValue || max == min);
        }

        public void ReadValue(PropertyConfiguration prop)
        {
            if (prop.VersionSpecificAttribute != null && Version != prop.VersionSpecificAttribute.Version)
                return;

            var offset = prop.OffsetAttributes.FirstOrDefault(ValidateVersion)?.Offset ?? throw new InvalidOperationException();
            Reader.Seek(Origin + offset, SeekOrigin.Begin);

            var byteOrder = prop.ByteOrderAttributes.FirstOrDefault(ValidateVersion)?.ByteOrder ?? ByteOrder;
            var storageType = prop.StoreTypeAttributes.FirstOrDefault(ValidateVersion)?.StoreType ?? prop.PropertyType;

            if (storageType.IsGenericType && storageType.GetGenericTypeDefinition() == typeof(Nullable<>))
                storageType = storageType.GetGenericArguments()[0];

            if (storageType.IsEnum)
                storageType = Enum.GetUnderlyingType(storageType);

            var value = ReadValue(prop, storageType, byteOrder);

            if (prop.IsVersionNumber && !Version.HasValue)
            {
                var version = value;
                if (Utils.TryConvert(ref version, storageType, typeof(double)))
                    Version = (double)version;
            }

            if (prop.DataLengthAttributes.Any(ValidateVersion))
            {
                var length = value;
                if (Utils.TryConvert(ref length, storageType, typeof(long)))
                    DataLength = (long)length;
            }

            prop.SetValue(Target, value);
        }

        private object ReadValue(PropertyConfiguration prop, Type storageType, ByteOrder byteOrder)
        {
            if (storageType == typeof(string))
            {
                if (prop.FixedLengthAttribute != null)
                    return Reader.ReadString(prop.FixedLengthAttribute.Length, prop.FixedLengthAttribute.Trim);
                else if (prop.IsLengthPrefixed)
                    return Reader.ReadString(byteOrder);
                else
                {
                    return prop.NullTerminatedAttribute.HasLength
                        ? Reader.ReadNullTerminatedString(prop.NullTerminatedAttribute.Length)
                        : Reader.ReadNullTerminatedString();
                }
            }
            else if (MethodCache.ReadMethods.ContainsKey(storageType))
                return MethodCache.ReadMethods[storageType].Invoke(Reader, byteOrder);

            return Version.HasValue
                ? Reader.ReadObject(storageType, Version.Value)
                : Reader.ReadObject(storageType);
        }

        public override string ToString() => $"{Target.GetType().Name} v{Version?.ToString() ?? "NULL"} @ {Origin}";
    }
}
