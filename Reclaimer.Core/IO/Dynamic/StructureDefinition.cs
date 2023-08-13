using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public class StructureDefinition<TClass>
    {
        private static StructureDefinition<TClass> instance;

        private readonly List<VersionDefinition> versions = new();

        public static void Populate(ref TClass value, EndianReader reader, double? version)
        {
            instance ??= FromAttributes();

            var definition = instance.versions.FirstOrDefault(d => d.ValidFor(version));
            definition.ReadValue(ref value, reader);
        }

        public static void Write(ref TClass value, EndianWriter writer, double? version)
        {
            instance ??= FromAttributes();

            var definition = instance.versions.FirstOrDefault(d => d.ValidFor(version));
            definition.WriteValue(ref value, writer);
        }

        public static StructureDefinition<TClass> FromAttributes()
        {
            var classAttributes = typeof(TClass).GetCustomAttributes().OfType<Attribute>().ToList();

            var properties = typeof(TClass).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(OffsetAttribute)))
                .ToList();

            var propAttributes = properties
                .ToDictionary(p => p, p => p.GetCustomAttributes().OfType<Attribute>().ToList());

            var result = new StructureDefinition<TClass>();

            foreach (var (min, max) in GetVersionRanges().Distinct())
            {
                var versionTest = min ?? (max - 1); //if min is unbound, just use a number < max (null if max also unbound)

                var byteOrder = classAttributes.OfType<ByteOrderAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(versionTest))?.ByteOrder;

                var size = classAttributes.OfType<FixedSizeAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(versionTest))?.Size;

                var def = new VersionDefinition(min, max, byteOrder, size);
                result.versions.Add(def);

                foreach (var prop in properties)
                {
                    var attributes = propAttributes[prop];

                    var propMin = attributes.OfType<MinVersionAttribute>().SingleOrDefault()?.MinVersion;
                    var propMax = attributes.OfType<MaxVersionAttribute>().SingleOrDefault()?.MaxVersion;
                    var propExact = attributes.OfType<VersionSpecificAttribute>().SingleOrDefault()?.Version;

                    if (!propExact.HasValue && propMin == propMax)
                        propExact = propMin;

                    if (!Extensions.ValidateVersion(versionTest, propMin, propMax))
                        continue;

                    if (propExact.HasValue && (propExact != min || propExact != max))
                        continue;

                    var offset = attributes.OfType<OffsetAttribute>().SingleOrDefault(a => a.ValidateVersion(versionTest))?.Offset;
                    if (!offset.HasValue)
                        continue;

                    byteOrder = attributes.OfType<ByteOrderAttribute>().SingleOrDefault(a => a.ValidateVersion(versionTest))?.ByteOrder;
                    var storeType = attributes.OfType<StoreTypeAttribute>().SingleOrDefault(a => a.ValidateVersion(versionTest))?.StoreType;
                    var field = FieldDefinition<TClass>.Create(prop, offset.Value, byteOrder, storeType);
                    def.AddField(field);
                }
            }

            return result;

            IEnumerable<(double?, double?)> GetVersionRanges()
            {
                bool hasUnboundedMin = false, hasUnboundedMax = false;
                foreach (var attrList in propAttributes.Values.Prepend(classAttributes))
                {
                    if (!hasUnboundedMin)
                    {
                        hasUnboundedMin = attrList.OfType<IVersionAttribute>().Any(v => v.HasMaxVersion && !v.HasMinVersion)
                            || (attrList.OfType<MaxVersionAttribute>().Any() && !attrList.OfType<MinVersionAttribute>().Any());
                    }

                    if (!hasUnboundedMax)
                    {
                        hasUnboundedMax = attrList.OfType<IVersionAttribute>().Any(v => v.HasMinVersion && !v.HasMaxVersion)
                            || (attrList.OfType<MinVersionAttribute>().Any() && !attrList.OfType<MaxVersionAttribute>().Any());
                    }

                    if (hasUnboundedMin && hasUnboundedMax)
                        break; //dont need to check any further
                }

                //get all the version boundaries from all version-related attributes on the class and its properties
                //for VersionSpecificAttribute, also add a second copy of the version number (after Distinct()) so there will be a range with min and max set to the same version
                var possibleVersions = propAttributes.SelectMany(kv => kv.Value)
                    .Union(classAttributes)
                    .SelectMany(GetVersions)
                    .Distinct()
                    .Concat(propAttributes.SelectMany(kv => kv.Value).OfType<VersionSpecificAttribute>().Select(a => (double?)a.Version).Distinct())
                    .Order()
                    .ToList();

                if (hasUnboundedMin)
                    possibleVersions.Insert(0, null);

                if (hasUnboundedMax)
                    possibleVersions.Add(null);

                //make a version range for each consecutive ascending pair of version boundaries
                for (var i = 0; i < possibleVersions.Count - 1; i++)
                    yield return (possibleVersions[i], possibleVersions[i + 1]);

                //default range always returned last so specific versions will be found first before falling back to default
                yield return (null, null);
            };

            static IEnumerable<double?> GetVersions(Attribute attribute)
            {
                if (attribute is IVersionAttribute versioned)
                {
                    if (versioned.HasMinVersion)
                        yield return versioned.MinVersion;

                    if (versioned.HasMaxVersion)
                        yield return versioned.MaxVersion;
                }
                else if (attribute is MinVersionAttribute min)
                    yield return min.MinVersion;
                else if (attribute is MaxVersionAttribute max)
                    yield return max.MaxVersion;
                else if (attribute is VersionSpecificAttribute spec)
                    yield return spec.Version;
            }
        }

        /// <summary>
        /// Contains the settings and field definitions to use when reading or writing a particular version of a <typeparamref name="TClass"/> value.
        /// </summary>
        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        private class VersionDefinition
        {
            private readonly List<FieldDefinition<TClass>> fields = new();

            private readonly double? minVersion;
            private readonly double? maxVersion;
            private readonly ByteOrder? byteOrder;
            private readonly long? size;

            public VersionDefinition(double? minVersion, double? maxVersion, ByteOrder? byteOrder, long? size)
            {
                this.minVersion = minVersion;
                this.maxVersion = maxVersion;
                this.byteOrder = byteOrder;
                this.size = size;
            }

            public bool ValidFor(double? version) => Extensions.ValidateVersion(version, minVersion, maxVersion);

            public void AddField(FieldDefinition<TClass> definition)
            {
                //ensure list stays in order of offset (not using SortedList because it doesnt allow duplicates)
                var index = fields.FindIndex(f => f.Offset > definition.Offset);
                if (index >= 0)
                    fields.Insert(index, definition);
                else
                    fields.Add(definition);
            }

            public void ReadValue(ref TClass value, EndianReader reader)
            {
                var origin = reader.Position;
                foreach (var field in fields)
                {
                    reader.Seek(origin + field.Offset, SeekOrigin.Begin);
                    field.ReadValue(ref value, reader, field.ByteOrder ?? byteOrder);
                }

                if (size.HasValue)
                    reader.Seek(origin + size.Value, SeekOrigin.Begin);
            }

            public void WriteValue(ref TClass value, EndianWriter writer)
            {
                var origin = writer.Position;
                foreach (var field in fields)
                {
                    writer.Seek(origin + field.Offset, SeekOrigin.Begin);
                    field.WriteValue(ref value, writer, field.ByteOrder ?? byteOrder);
                }

                if (size.HasValue)
                    writer.Seek(origin + size.Value, SeekOrigin.Begin);
            }

            private string GetDebuggerDisplay() => minVersion.HasValue || maxVersion.HasValue ? new { Min = minVersion, Max = maxVersion }.ToString() : "{Default}";
        }
    }
}
