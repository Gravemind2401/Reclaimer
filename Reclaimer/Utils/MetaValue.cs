using Adjutant.Blam.Common;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Windows;

namespace Reclaimer.Utils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class MetaValueTypeAliasAttribute : Attribute
    {
        public string Alias { get; }

        public MetaValueTypeAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public class MetaValue : BindableBase
    {
        private ICacheFile cache;
        private XmlNode node;

        public string Name { get; }
        public string Description { get; }
        public string Label { get; }
        public int Length { get; }
        public bool IsVisible { get; }

        public long BaseAddress { get; }
        public int Offset { get; }
        public int BlockSize { get; }

        public MetaValueType ValueType { get; }

        public long ValueAddress => BaseAddress + Offset;

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        private object _value;
        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private int blockIndex;
        public int BlockIndex
        {
            get { return blockIndex; }
            set
            {
                if (SetProperty(ref blockIndex, value))
                    ReadValue();
            }
        }

        private IEnumerable<string> blockLabels;
        public IEnumerable<string> BlockLabels
        {
            get { return blockLabels; }
            set { SetProperty(ref blockLabels, value); }
        }

        public ObservableCollection<MetaValue> Children { get; }

        public MetaValue(XmlNode node, ICacheFile cache, long metaAddress)
            : this(node, cache, metaAddress, cache.CreateReader(cache.DefaultAddressTranslator), false)
        {

        }

        private MetaValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader, bool leaveOpen)
        {
            this.cache = cache;
            this.node = node;

            BaseAddress = baseAddress;

            Name = node.Attributes["name"]?.Value;
            Description = node.Attributes["desc"]?.Value;
            Label = node.Attributes["label"]?.Value;
            Length = GetIntAttribute(node, "length", 0);
            IsVisible = GetBoolAttribute(node, "visible", true);

            Offset = GetIntAttribute(node, "offset", 0);
            BlockSize = GetIntAttribute(node, "size", 0);

            ValueType = GetMetaValueType(node.Name);

            Children = new ObservableCollection<MetaValue>();

            ReadValue(reader, leaveOpen);
        }

        private void ReadValue() => ReadValue(cache.CreateReader(cache.DefaultAddressTranslator), false);

        private void ReadValue(EndianReader reader, bool leaveOpen)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                switch (ValueType)
                {
                    case MetaValueType.Undefined:
                    default:
                        Value = reader.ReadInt32();
                        break;

                    case MetaValueType.Struct: ReadStruct(reader); break;
                    case MetaValueType.StringId: ReadStringId(reader); break;
                    //case MetaValueType.TagRef: LoadTagRef(reader); break;

                    //case MetaValueType.Comment: Value = metaNode.InnerText; break;

                    case MetaValueType.String: Value = reader.ReadNullTerminatedString(Length); break;

                    case MetaValueType.Float32: Value = reader.ReadSingle(); break;

                    case MetaValueType.Int8: Value = reader.ReadByte(); break;
                    case MetaValueType.Int16: Value = reader.ReadInt16(); break;
                    case MetaValueType.Int32: Value = reader.ReadInt32(); break;
                    case MetaValueType.Int64: Value = reader.ReadInt64(); break;

                    case MetaValueType.UInt16: Value = reader.ReadUInt16(); break;
                    case MetaValueType.UInt32: Value = reader.ReadUInt32(); break;
                    case MetaValueType.UInt64: Value = reader.ReadUInt64(); break;

                    //case MetaValueType.Bitmask8:
                    //case MetaValueType.Bitmask16:
                    //case MetaValueType.Bitmask32:
                    //case MetaValueType.Enum8:
                    //case MetaValueType.Enum16:
                    //case MetaValueType.Enum32:
                    //    LoadOptions(reader);
                    //    break;

                    //case MetaValueType.ShortBounds:
                    //case MetaValueType.ShortPoint2D:
                    //    Value = new RealQuat(reader.ReadInt16(), reader.ReadInt16());
                    //    break;

                    case MetaValueType.RealBounds:
                    case MetaValueType.RealPoint2D:
                    case MetaValueType.RealVector2D:
                        Value = new RealVector2D(reader.ReadSingle(), reader.ReadSingle());
                        break;

                    case MetaValueType.RealPoint3D:
                    case MetaValueType.RealVector3D:
                        Value = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;

                    case MetaValueType.RealPoint4D:
                    case MetaValueType.RealVector4D:
                        Value = new RealVector4D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;
                }

                if (!leaveOpen)
                    reader.Dispose();
            }
            catch { IsEnabled = false; }
        }

        private void ReadStringId(EndianReader reader)
        {
            reader.Seek(ValueAddress, SeekOrigin.Begin);
            var id = cache.CacheType < CacheType.Halo3Beta ? reader.ReadInt16() : reader.ReadInt32();
            Value = cache.StringIndex[id];
        }

        private void ReadStruct(EndianReader reader)
        {
            Children.Clear();

            reader.Seek(ValueAddress, SeekOrigin.Begin);
            var count = reader.ReadInt32();
            var pointer = new Pointer(reader.ReadInt32(), cache.DefaultAddressTranslator);

            BlockLabels = count > 0
                ? Enumerable.Range(0, count).Select(i => $"Block {i:D3}")
                : Enumerable.Empty<string>();

            if (count <= 0)
            {
                IsEnabled = false;
                return;
            }

            foreach (XmlNode n in node.ChildNodes)
                Children.Add(new MetaValue(n, cache, pointer.Address + BlockIndex * BlockSize, reader, true));
        }

        private static int GetIntAttribute(XmlNode node, string valueName, int defaultValue)
        {
            if (node.Attributes[valueName] == null) return defaultValue;

            int intVal;
            var strVal = node.Attributes[valueName].Value;

            if (int.TryParse(strVal, out intVal))
                return intVal;
            else if (int.TryParse(strVal.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out intVal))
                return intVal;
            else return defaultValue;
        }

        private static bool GetBoolAttribute(XmlNode node, string valueName, bool defaultValue)
        {
            if (node.Attributes[valueName] == null) return defaultValue;

            bool bVal;
            var strVal = node.Attributes[valueName].Value;

            if (bool.TryParse(strVal, out bVal))
                return bVal;
            else return defaultValue;
        }

        private static readonly Dictionary<string, MetaValueType> typeLookup = new Dictionary<string, MetaValueType>();
        private static MetaValueType GetMetaValueType(string typeName)
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
                    if (attr.Alias == typeName)
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

    public enum MetaValueType
    {
        [MetaValueTypeAlias("reflexive")]
        Struct,
        StructureGroup,

        [MetaValueTypeAlias("tagreference")]
        TagRef,

        StringId,
        String,

        Bitmask8,
        Bitmask16,
        Bitmask32,

        Comment,

        DataRef,

        [MetaValueTypeAlias("float")]
        Float32,

        [MetaValueTypeAlias("byte")]
        Int8,
        Int16,
        Int32,
        Int64,

        UInt8,
        UInt16,
        UInt32,
        UInt64,

        RawID,

        Enum8,
        Enum16,
        Enum32,

        Undefined,

        ShortBounds,
        RealBounds,

        ShortPoint2D,
        RealPoint2D,
        RealPoint3D,
        RealPoint4D,

        RealVector2D,
        RealVector3D,
        RealVector4D,

        Colour32RGB,
        Colour32ARGB,
    }

    public class MetaValueTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var meta = item as MetaValue;

            if (element == null || meta == null)
                return base.SelectTemplate(item, container);

            switch (meta.ValueType)
            {
                case MetaValueType.Struct:
                    return element.FindResource("StructureTemplate") as DataTemplate;

                default:
                    return element.FindResource("DefaultTemplate") as DataTemplate;
            }
        }
    }
}
