using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Reclaimer.Utilities;
using System.Runtime.CompilerServices;

namespace Reclaimer.Plugins.MetaViewer
{
    public abstract class MetaValue : BindableBase
    {
        protected readonly ICacheFile cache;
        protected readonly XmlNode node;

        public string Name { get; }
        public int Offset { get; }
        public string ToolTip { get; }
        public string Description { get; }
        public bool IsVisible { get; }

        public long BaseAddress { get; internal set; }

        public MetaValueType ValueType { get; }
        public long ValueAddress => BaseAddress + Offset;

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        public virtual string EntryString => null;

        public bool IsDirty { get; protected set; }

        protected MetaValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
        {
            this.cache = cache;
            this.node = node;

            BaseAddress = baseAddress;

            Name = node.Attributes["name"]?.Value;
            Description = GetStringAttribute(node, "description", "desc");
            Offset = GetIntAttribute(node, "offset") ?? 0;
            ToolTip = GetStringAttribute(node, "tooltip");
            IsVisible = GetBoolAttribute(node, "visible") ?? false;

            ValueType = GetMetaValueType(node.Name);
        }

        protected bool SetMetaProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = SetProperty(ref property, value, propertyName);
            if (changed) IsDirty = true;
            return changed;
        }

        public static MetaValue GetValue(XmlNode node, ICacheFile cache, long baseAddress)
        {
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                reader.Seek(baseAddress, SeekOrigin.Begin);

                switch (GetMetaValueType(node.Name))
                {
                    case MetaValueType.Structure:
                        return new StructureValue(node, cache, baseAddress, reader);

                    case MetaValueType.String:
                    case MetaValueType.StringId:
                        return new StringValue(node, cache, baseAddress, reader);

                    case MetaValueType.TagRef:
                        return new TagReferenceValue(node, cache, baseAddress, reader);

                    case MetaValueType.Comment:
                        return new CommentValue(node, cache, baseAddress, reader);

                    case MetaValueType.Bitmask8:
                    case MetaValueType.Bitmask16:
                    case MetaValueType.Bitmask32:
                        return new BitmaskValue(node, cache, baseAddress, reader);

                    case MetaValueType.Enum8:
                    case MetaValueType.Enum16:
                    case MetaValueType.Enum32:
                        return new EnumValue(node, cache, baseAddress, reader);

                    //case MetaValueType.ShortBounds:
                    //case MetaValueType.ShortPoint2D:
                    case MetaValueType.RealBounds:
                    case MetaValueType.RealPoint2D:
                    case MetaValueType.RealPoint3D:
                    case MetaValueType.RealPoint4D:
                    case MetaValueType.RealVector2D:
                    case MetaValueType.RealVector3D:
                    case MetaValueType.RealVector4D:
                        return new MultiValue(node, cache, baseAddress, reader);

                    default: return new SimpleValue(node, cache, baseAddress, reader);
                }
            }
        }

        public abstract void ReadValue(EndianReader reader);

        public abstract void WriteValue(EndianWriter writer);

        protected static string GetStringAttribute(XmlNode node, params string[] possibleNames)
        {
            return FindAttribute(node, possibleNames)?.Value;
        }

        protected static int? GetIntAttribute(XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null) return null;

            int intVal;
            var strVal = attr.Value;

            if (int.TryParse(strVal, out intVal))
                return intVal;
            else if (int.TryParse(strVal.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out intVal))
                return intVal;
            else return null;
        }

        protected static bool? GetBoolAttribute(XmlNode node, params string[] possibleNames)
        {
            var attr = FindAttribute(node, possibleNames);
            if (attr == null) return null;

            bool boolVal;
            var strVal = attr.Value;

            if (bool.TryParse(strVal, out boolVal))
                return boolVal;
            else return null;
        }

        protected static XmlAttribute FindAttribute(XmlNode node, params string[] possibleNames)
        {
            return node.Attributes.Cast<XmlAttribute>().FirstOrDefault(a => possibleNames.Any(s => s.ToUpper() == a.Name.ToUpper()));
        }

        private static readonly Dictionary<string, MetaValueType> typeLookup = new Dictionary<string, MetaValueType>();
        protected static MetaValueType GetMetaValueType(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            typeName = typeName.ToUpper();
            if (typeLookup.ContainsKey(typeName))
                return typeLookup[typeName];

            var result = MetaValueType.Undefined;
            if (Enum.TryParse(typeName, true, out result))
            {
                typeLookup.Add(typeName, result);
                return result;
            }

            foreach (var fi in typeof(MetaValueType).GetFields().Where(f => f.FieldType == typeof(MetaValueType)))
            {
                foreach (MetaValueTypeAliasAttribute attr in fi.GetCustomAttributes(typeof(MetaValueTypeAliasAttribute), false))
                {
                    if (attr.Alias.ToUpper() == typeName)
                    {
                        result = (MetaValueType)fi.GetValue(null);
                        typeLookup.Add(typeName, result);
                        return result;
                    }
                }
            }

            typeLookup.Add(typeName, MetaValueType.Undefined);
            return MetaValueType.Undefined;
        }
    }
}
