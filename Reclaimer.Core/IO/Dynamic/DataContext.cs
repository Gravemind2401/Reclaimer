using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Reclaimer.IO.Dynamic
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class DataContext
    {
        public object Target { get; }
        public long Origin { get; }
        public IEndianStream Stream { get; }
        public EndianReader Reader { get; }
        public EndianWriter Writer { get; }
        public ByteOrder ByteOrder { get; }
        public double? Version { get; set; }
        public long? DataLength { get; set; }

        private DataContext(TypeConfiguration manager, object instance, double? version, IEndianStream stream)
        {
            Target = instance;
            Origin = stream.Position;
            Stream = stream;
            Version = version;
            ByteOrder = manager.ByteOrderAttributes.FirstOrDefault(ValidateVersion)?.ByteOrder ?? stream.ByteOrder;
        }

        public DataContext(TypeConfiguration manager, object instance, double? version, EndianReader reader)
            : this(manager, instance, version, (IEndianStream)reader)
        {
            Reader = reader;
        }

        public DataContext(TypeConfiguration manager, object instance, double? version, EndianWriter writer)
            : this(manager, instance, version, (IEndianStream)writer)
        {
            Writer = writer;
        }

        public bool ValidateVersion(PropertyConfiguration property) => ValidateVersion(Version, property.MinVersionAttribute?.MinVersion, property.MaxVersionAttribute?.MaxVersion);
        public bool ValidateVersion(IVersionAttribute attr) => ValidateVersion(Version, attr.HasMinVersion ? attr.MinVersion : null, attr.HasMaxVersion ? attr.MaxVersion : null);
        private static bool ValidateVersion(double? version, double? min, double? max)
        {
            return (version >= min || !min.HasValue) && (version < max || !max.HasValue || max == min);
        }

        private Type GetStorageType(PropertyConfiguration prop)
        {
            var storageType = prop.StoreTypeAttributes.FirstOrDefault(ValidateVersion)?.StoreType ?? prop.PropertyType;

            if (storageType.IsGenericType && storageType.GetGenericTypeDefinition() == typeof(Nullable<>))
                storageType = storageType.GetGenericArguments()[0];

            if (storageType.IsEnum)
                storageType = Enum.GetUnderlyingType(storageType);

            return storageType;
        }

        public void SeekOffset(long offset) => Stream.Seek(Origin + offset, SeekOrigin.Begin);

        public void ReadValue(PropertyConfiguration prop)
        {
            var offset = prop.OffsetAttributes.FirstOrDefault(ValidateVersion)?.Offset ?? throw new InvalidOperationException();
            SeekOffset(offset);

            var byteOrder = prop.ByteOrderAttributes.FirstOrDefault(ValidateVersion)?.ByteOrder ?? ByteOrder;
            var storageType = GetStorageType(prop);
            var value = ReadValue(prop, storageType, byteOrder);

            if (storageType == typeof(string) && prop.IsInterned)
                value = string.Intern((string)value);

            if (prop.IsVersionNumber && !Version.HasValue && value != null)
                Version = Convert.ToDouble(value);

            if (prop.DataLengthAttributes.Any(ValidateVersion) && value != null)
                DataLength = Convert.ToInt64(value);

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
            
            if (MethodCache.ReadMethods.ContainsKey(storageType))
                return MethodCache.ReadMethods[storageType].Invoke(Reader, byteOrder);

            return Version.HasValue
                ? Reader.ReadObject(storageType, Version.Value)
                : Reader.ReadObject(storageType);
        }

        public void WriteValue(PropertyConfiguration prop)
        {
            var offset = prop.OffsetAttributes.FirstOrDefault(ValidateVersion)?.Offset ?? throw new InvalidOperationException();
            SeekOffset(offset);

            var byteOrder = prop.ByteOrderAttributes.FirstOrDefault(ValidateVersion)?.ByteOrder ?? ByteOrder;
            var storageType = GetStorageType(prop);
            var value = prop.GetValue(Target);

            if (prop.IsVersionNumber && Version.HasValue)
            {
                value = Version;
                if (!Utils.TryConvert(ref value, typeof(double), storageType))
                    throw new InvalidCastException($"The version number provided in place of {prop.Property.Name} could not be stored as {storageType}");
            }

            if (value == null && !storageType.IsValueType)
                throw new InvalidOperationException($"Null reference types cannot be written to stream");

            if (value != null && prop.PropertyType != storageType && !Utils.TryConvert(ref value, prop.PropertyType, storageType))
                throw new InvalidCastException($"The value in {prop.Property.Name} could not be stored as {storageType}");

            //for nullable types write the default value
            WriteValue(prop, storageType, byteOrder, value ?? Activator.CreateInstance(storageType));

            if (value != null && prop.DataLengthAttributes.Any(ValidateVersion))
                DataLength = Convert.ToInt64(value);
        }

        private void WriteValue(PropertyConfiguration prop, Type storageType, ByteOrder byteOrder, object value)
        {
            if (storageType == typeof(string))
            {
                var stringValue = (string)value;
                if (prop.FixedLengthAttribute != null)
                    Writer.WriteStringFixedLength(stringValue, prop.FixedLengthAttribute.Length, prop.FixedLengthAttribute.Padding);
                else if (prop.IsLengthPrefixed)
                    Writer.Write(stringValue, byteOrder);
                else
                {
                    if (prop.NullTerminatedAttribute.HasLength && stringValue?.Length > prop.NullTerminatedAttribute.Length)
                        stringValue = stringValue[..prop.NullTerminatedAttribute.Length];
                    Writer.WriteStringNullTerminated(stringValue);
                }
            }
            else if (MethodCache.WriteMethods.ContainsKey(storageType))
                MethodCache.WriteMethods[storageType].Invoke(Writer, byteOrder, value);
            else if (Version.HasValue)
                Writer.WriteObject(value, Version.Value);
            else
                Writer.WriteObject(storageType);
        }

        private string GetDebuggerDisplay() => $"{Target.GetType().Name} v{Version?.ToString() ?? "NULL"} @ {Origin}";
    }
}
