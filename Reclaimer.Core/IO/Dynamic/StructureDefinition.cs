using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Reclaimer.IO.Dynamic
{
    public class StructureDefinition<TClass>
    {
        private static StructureDefinition<TClass> instance;

        private readonly List<VersionDefinition> versions = new();

        public static TClass Populate(ref TClass value, EndianReader reader, double? version)
        {
            instance ??= FromAttributes();

            var definition = instance.versions.FirstOrDefault(d => d.ValidFor(version));
            definition.ReadValue(ref value, reader);
            return value;
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
                var byteOrder = classAttributes.OfType<ByteOrderAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(min))?.ByteOrder;

                var size = classAttributes.OfType<FixedSizeAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(min))?.Size;

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

                    if (!Extensions.ValidateVersion(min, propMin, propMax))
                        continue;

                    if (propExact.HasValue && (propExact != min || propExact != max))
                        continue;

                    var offset = attributes.OfType<OffsetAttribute>().SingleOrDefault(a => a.ValidateVersion(min))?.Offset;
                    if (!offset.HasValue)
                        continue;

                    byteOrder = attributes.OfType<ByteOrderAttribute>().SingleOrDefault(a => a.ValidateVersion(min))?.ByteOrder;
                    var storeType = attributes.OfType<StoreTypeAttribute>().SingleOrDefault(a => a.ValidateVersion(min))?.StoreType;
                    var field = FieldDefinition<TClass>.Create(prop, offset.Value, byteOrder, storeType);
                    def.AddField(field);
                }
            }

            return result;

            IEnumerable<(double?, double?)> GetVersionRanges()
            {
                var possibleVersions = propAttributes.SelectMany(kv => kv.Value)
                    .Union(classAttributes)
                    .SelectMany(GetVersions)
                    .Distinct()
                    .Concat(propAttributes.SelectMany(kv => kv.Value).OfType<VersionSpecificAttribute>().Select(a => (double?)a.Version).Distinct())
                    .Order()
                    .Append(null) //final range will always have null max version
                    .ToList();

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

                    if (!versioned.IsVersioned)
                        yield return default;
                }
                else if (attribute is MinVersionAttribute min)
                    yield return min.MinVersion;
                else if (attribute is MaxVersionAttribute max)
                    yield return max.MaxVersion;
                else if (attribute is VersionSpecificAttribute spec)
                    yield return spec.Version;
            }
        }

        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        internal class VersionDefinition
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

            private string GetDebuggerDisplay() => new { Min = minVersion, Max = maxVersion }.ToString();
        }
    }
}
