using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Reclaimer.IO.Dynamic
{
    internal static class StructureDefinition
    {
        //this class is for debugging purposes to help inspect structure definitions

        public static readonly Dictionary<Type, IStructureDefinition> InstanceLookup = new();

        public static IEnumerable<IStructureDefinition> Instances => InstanceLookup.Values.OrderBy(s => IStructureDefinition.GetTypeDisplayName(s.TargetType));

        [Conditional("DEBUG")]
        public static void AddInstance(IStructureDefinition definition)
        {
            InstanceLookup[definition.TargetType] = definition;

            Task.Run(() =>
            {
                var dir = Path.GetDirectoryName(typeof(StructureDefinition).Assembly.Location);
                dir = Path.Combine(dir, "StructureDefinitions");
                Directory.CreateDirectory(dir);

                var fileName = string.Join("_", IStructureDefinition.GetTypeDisplayName(definition.TargetType).Replace('<', '[').Replace('>', ']').Split(Path.GetInvalidFileNameChars()));
                fileName = Path.Combine(dir, fileName + ".json");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = new JsonObject();

                foreach (var v in definition.Versions.Reverse())
                {
                    var name = v.MinVersion.HasValue && v.MaxVersion.HasValue
                        ? $"{v.MinVersionDisplay}..{v.MaxVersionDisplay}"
                        : v.MinVersion.HasValue || v.MaxVersion.HasValue
                            ? (v.MinVersionDisplay ?? "..") + (v.MaxVersionDisplay ?? "..")
                            : "default";

                    var node = JsonSerializer.SerializeToNode(new
                    {
                        v.ByteOrder,
                        FixedSize = v.Size,
                        VersionField = v.VersionField?.TargetProperty.Name,
                        DataLengthField = v.DataLengthField?.TargetProperty.Name
                    }, options) as JsonObject;

                    var fields = from f in v.Fields
                                 let props = new
                                 {
                                     Type = IStructureDefinition.GetTypeDisplayName(f.TargetType),
                                     f.TargetProperty.Name,
                                     f.ByteOrder
                                 }
                                 orderby f.Offset
                                 group props by f.Offset into g
                                 select new KeyValuePair<string, JsonNode>(g.Key.ToString(), JsonSerializer.SerializeToNode(g, options));

                    node.Add(nameof(v.Fields), new JsonObject(fields));
                    json.Add(name, node);
                }

                File.WriteAllText(fileName, json.ToJsonString(options));
            });
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class StructureDefinition<TClass> : IStructureDefinition
    {
        #region IStructureDefinition

        Type IStructureDefinition.TargetType => typeof(TClass);
        IEnumerable<IVersionDefinition> IStructureDefinition.Versions => versions;

        #endregion

        #region Static Members

        private static readonly StructureDefinition<TClass> instance;

        static StructureDefinition()
        {
            var attr = typeof(TClass).GetCustomAttribute<StructureDefinitionAttribute<TClass>>();
            instance = attr?.GetStructureDefinition() ?? FromAttributes();
        }

        public static void Populate(ref TClass value, EndianReader reader, ref double? version, in long origin)
        {
            //if a version was provided, always use it
            //otherwise, try to use the version from the version field, if applicable
            if (!version.HasValue)
            {
                var defaultVersion = instance.versions[^1];
                var versionField = defaultVersion.VersionField;
                if (versionField != null)
                {
                    reader.Seek(versionField.Offset, SeekOrigin.Begin);
                    version = versionField.StreamReadVersionField(reader, versionField.ByteOrder ?? defaultVersion.ByteOrder);
                    reader.Seek(origin, SeekOrigin.Begin);
                }
            }

            var definition = FindVersionDefinition(version);

            foreach (var field in definition.Fields)
            {
                reader.Seek(origin + field.Offset, SeekOrigin.Begin);
                field.ReadValue(ref value, reader, field.ByteOrder ?? definition.ByteOrder);
            }

            if (definition.Size.HasValue)
                reader.Seek(origin + definition.Size.Value, SeekOrigin.Begin);
            else if (definition.DataLengthField != null)
            {
                var size = Convert.ToInt64(definition.DataLengthField.TargetProperty.GetValue(value));
                Exceptions.ThrowIfNotPositive(size, definition.DataLengthField.TargetProperty.Name);
                reader.Seek(origin + size, SeekOrigin.Begin);
            }
        }

        public static void Write(ref TClass value, EndianWriter writer, ref double? version)
        {
            var origin = writer.Position;

            //if a version was provided, always use it
            //otherwise, try to use the version from the version field, if applicable
            if (!version.HasValue)
            {
                var defaultVersion = instance.versions[^1];
                var versionField = defaultVersion.VersionField;
                if (versionField != null)
                    version = Convert.ToDouble(versionField.TargetProperty.GetValue(value));
            }

            var definition = FindVersionDefinition(version);
            foreach (var field in definition.Fields)
            {
                var byteOrder = field.ByteOrder ?? definition.ByteOrder;

                writer.Seek(origin + field.Offset, SeekOrigin.Begin);
                if (field.IsVersionProperty)
                {
                    //ensure that the version written to stream always reflects the version the structure was written with
                    field.StreamWriteVersionField(writer, version.GetValueOrDefault(), byteOrder);
                }
                else
                    field.WriteValue(ref value, writer, byteOrder);
            }

            if (definition.Size.HasValue)
                writer.Seek(origin + definition.Size.Value, SeekOrigin.Begin);
            else if (definition.DataLengthField != null)
            {
                var size = Convert.ToInt64(definition.DataLengthField.TargetProperty.GetValue(value));
                Exceptions.ThrowIfNotPositive(size, definition.DataLengthField.TargetProperty.Name);
                writer.Seek(origin + size, SeekOrigin.Begin);
            }
        }

        private static VersionDefinition FindVersionDefinition(double? version)
        {
            return instance.versions.First(d => Extensions.ValidateVersion(version, d.MinVersion, d.MaxVersion));
        }

        private static StructureDefinition<TClass> FromAttributes()
        {
            var classAttributes = typeof(TClass).GetCustomAttributes().OfType<Attribute>().ToList();

            var properties = typeof(TClass).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(OffsetAttribute)))
                .ToList();

            var propAttributes = properties
                .ToDictionary(p => p, p => p.GetCustomAttributes().OfType<Attribute>().ToList());

            #region Attribute Validation

            foreach (var g in classAttributes.OfType<IVersionAttribute>().GroupBy(a => a.GetType()))
            {
                if (!g.ValidateOverlap())
                    throw Exceptions.AttributeVersionOverlap(typeof(TClass), g.Key);
            }

            if (propAttributes.SelectMany(kv => kv.Value).OfType<VersionNumberAttribute>().Skip(1).Any())
                throw Exceptions.MultipleVersionsSpecified(typeof(TClass));

            foreach (var (prop, attributes) in propAttributes)
            {
                if (prop.GetGetMethod() == null || prop.GetSetMethod() == null)
                    throw Exceptions.NonPublicGetSet(prop);

                foreach (var g in attributes.OfType<IVersionAttribute>().GroupBy(a => a.GetType()))
                {
                    if (!g.ValidateOverlap())
                        throw Exceptions.AttributeVersionOverlap(prop, g.Key);
                }

                if (attributes.OfType<IStringTypeAttribute>().Skip(1).Any())
                    throw Exceptions.StringTypeOverlap(prop);
            }

            #endregion

            var versions = new List<VersionDefinition>();

            foreach (var (min, max) in GetVersionRanges().Distinct())
            {
                var versionTest = min ?? (max - 1); //if min is unbound, just use a number < max (null if max also unbound)

                var byteOrder = classAttributes.OfType<ByteOrderAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(versionTest))?.ByteOrder;

                var size = classAttributes.OfType<FixedSizeAttribute>()
                    .SingleOrDefault(a => a.ValidateVersion(versionTest))?.Size;

                var def = new VersionDefinition(min, max, byteOrder, size);
                versions.Add(def);

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

            return new StructureDefinition<TClass>(versions);

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

        private static string GetDebuggerDisplay() => $"{{{IStructureDefinition.GetTypeDisplayName(typeof(TClass))}}}";

        #endregion

        #region Instance Members

        private readonly List<VersionDefinition> versions = new();

        internal StructureDefinition(IEnumerable<VersionDefinition> versions)
        {
            //sort by exact versions descending, then ranges descending, then null version last
            //this ensure the first match found will be the highest possible matching version (in case of multiple matches)
            var sorted = versions
                .OrderByDescending(v => v.MinVersion.HasValue || v.MaxVersion.HasValue) //null range last
                .ThenByDescending(v => v.MinVersion == v.MaxVersion) //exact versions first
                .ThenByDescending(v => v.MinVersion.HasValue) //null MinVersion last
                .ThenByDescending(v => v.MinVersion)
                .ThenBy(v => v.MaxVersion.HasValue) //null MaxVersion first
                .ThenByDescending(v => v.MaxVersion);

            this.versions.AddRange(sorted);

            StructureDefinition.AddInstance(this);
        }

        #endregion

        /// <summary>
        /// Contains the settings and field definitions to use when reading or writing a particular version of a <typeparamref name="TClass"/> value.
        /// </summary>
        [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
        public sealed class VersionDefinition : IVersionDefinition
        {
            #region IVersionDefinition

            IEnumerable<IFieldDefinition> IVersionDefinition.Fields => Fields;
            IFieldDefinition IVersionDefinition.VersionField => VersionField;
            IFieldDefinition IVersionDefinition.DataLengthField => DataLengthField;

            #endregion

            private readonly List<FieldDefinition<TClass>> fields = new();
            private readonly string minVersionDisplay;
            private readonly string maxVersionDisplay;

            public IReadOnlyList<FieldDefinition<TClass>> Fields { get; }
            public double? MinVersion { get; }
            public double? MaxVersion { get; }
            public ByteOrder? ByteOrder { get; }
            public long? Size { get; }

            public string MinVersionDisplay => minVersionDisplay ?? MinVersion?.ToString();
            public string MaxVersionDisplay => maxVersionDisplay ?? MaxVersion?.ToString();

            public FieldDefinition<TClass> VersionField { get; private set; }
            public FieldDefinition<TClass> DataLengthField { get; private set; }

            public VersionDefinition(double? minVersion, double? maxVersion, ByteOrder? byteOrder, long? size, string minVersionDisplay = null, string maxVersionDisplay = null)
            {
                fields = new();
                Fields = fields.AsReadOnly();
                MinVersion = minVersion;
                MaxVersion = maxVersion;
                ByteOrder = byteOrder;
                Size = size;

                this.minVersionDisplay = minVersionDisplay;
                this.maxVersionDisplay = maxVersionDisplay;
            }

            public void AddField(FieldDefinition<TClass> definition)
            {
                if (definition.IsVersionProperty)
                {
                    if (VersionField != null)
                        throw new InvalidDataException($"{nameof(VersionNumberAttribute)} can only be applied to one property at a time.");
                    VersionField = definition;
                }

                if (definition.IsDataLengthProperty)
                {
                    if (DataLengthField != null)
                        throw new InvalidDataException($"{nameof(DataLengthAttribute)} can only be applied to one property at a time.");
                    DataLengthField = definition;
                }

                //ensure list stays in order of offset (not using SortedList because it doesnt allow duplicates)
                var index = fields.FindIndex(f => f.Offset > definition.Offset);
                if (index >= 0)
                    fields.Insert(index, definition);
                else
                    fields.Add(definition);
            }

            private string GetDebuggerDisplay() => MinVersion.HasValue || MaxVersion.HasValue ? new { Min = MinVersionDisplay, Max = MaxVersionDisplay }.ToString() : "{Default}";
        }
    }
}
