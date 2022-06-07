using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    internal class TypeConfiguration
    {
        private static readonly Dictionary<Type, TypeConfiguration> instances = new();

        private readonly Type targetType;
        private readonly PropertyConfiguration versionProperty;
        private readonly List<PropertyConfiguration> properties;

        public IReadOnlyList<FixedSizeAttribute> FixedSizeAttributes { get; }
        public IReadOnlyList<ByteOrderAttribute> ByteOrderAttributes { get; }

        private TypeConfiguration(Type type)
        {
            targetType = type;

            FixedSizeAttributes = type.GetCustomAttributes<FixedSizeAttribute>().ToList();
            ByteOrderAttributes = type.GetCustomAttributes<ByteOrderAttribute>().ToList();

            if (!FixedSizeAttributes.ValidateOverlap())
                throw new InvalidOperationException();

            properties = type.GetProperties()
                .Select(p => new PropertyConfiguration(p))
                .Where(p => p.Validate())
                .ToList();

            if (!properties.SelectMany(p => p.DataLengthAttributes).ValidateOverlap())
                throw new InvalidOperationException();

            versionProperty = properties.SingleOrDefault(p => p.IsVersionNumber);
            if (versionProperty != null)
                properties.Remove(versionProperty);
        }

        private static TypeConfiguration ForType(Type type)
        {
            if (!instances.ContainsKey(type))
                instances.Add(type, new TypeConfiguration(type));
            return instances[type];
        }

        public static object Populate(object obj, Type type, EndianReader reader, double? version)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (MethodCache.ReadMethods.ContainsKey(type))
                return MethodCache.ReadMethods[type].Invoke(reader, reader.ByteOrder);

            return TypeConfiguration.ForType(type).PopulateInternal(obj, reader, version);
        }

        private object PopulateInternal(object obj, EndianReader reader, double? version)
        {
            if (!version.HasValue && FixedSizeAttributes.AllNotEmpty(Extensions.IsVersioned))
                throw new InvalidOperationException();

            var context = new DataContext(this, obj ?? Activator.CreateInstance(targetType), version, reader);

            if (versionProperty != null)
                context.ReadValue(versionProperty);

            foreach (var property in properties.Where(context.ValidateVersion).OrderBy(p => p.OffsetAttributes.FirstOrDefault(context.ValidateVersion)?.Offset))
                context.ReadValue(property);

            var fixedSize = FixedSizeAttributes.FirstOrDefault(context.ValidateVersion)?.Size ?? context.DataLength;
            if (fixedSize.HasValue)
                reader.Seek(context.Origin + fixedSize.Value, SeekOrigin.Begin);

            return context.Target;
        }
    }
}
