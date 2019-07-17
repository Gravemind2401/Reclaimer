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

                    //case MetaValueType.TagRef: LoadTagRef(reader); break;

                    //case MetaValueType.Comment: Value = metaNode.InnerText; break;

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

                    //case MetaValueType.RealBounds:
                    //case MetaValueType.RealPoint2D:
                    //case MetaValueType.RealVector2D:
                    //    Value = new RealVector2D(reader.ReadSingle(), reader.ReadSingle());
                    //    break;

                    //case MetaValueType.RealPoint3D:
                    //case MetaValueType.RealVector3D:
                    //    Value = new RealVector3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    //    break;

                    //case MetaValueType.RealPoint4D:
                    //case MetaValueType.RealVector4D:
                    //    Value = new RealVector4D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    //    break;

                    default: return new SimpleValue(node, cache, baseAddress, reader);
                }
            }
        }

        internal abstract void ReadValue(EndianReader reader);

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

    public class StructureValue : MetaValue
    {
        public int BlockSize { get; }

        private int blockCount;
        public int BlockCount
        {
            get { return blockCount; }
            set { SetProperty(ref blockCount, value); }
        }

        private int blockAddress;
        public int BlockAddress
        {
            get { return blockAddress; }
            set { SetProperty(ref blockAddress, value); }
        }

        private int blockIndex;
        public int BlockIndex
        {
            get { return blockIndex; }
            set
            {
                value = Math.Min(Math.Max(0, value), BlockCount - 1);

                if (SetProperty(ref blockIndex, value))
                    RefreshChildren();
            }
        }

        private IEnumerable<string> blockLabels;
        public IEnumerable<string> BlockLabels
        {
            get { return blockLabels; }
            set { SetProperty(ref blockLabels, value); }
        }

        public ObservableCollection<MetaValue> Children { get; }

        public StructureValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            BlockSize = GetIntAttribute(node, "entrySize", "size") ?? 0;
            Children = new ObservableCollection<MetaValue>();
            ReadValue(reader);
        }

        internal override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                Children.Clear();
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                BlockCount = reader.ReadInt32();
                BlockAddress = new Pointer(reader.ReadInt32(), cache.DefaultAddressTranslator).Address;

                if (BlockCount <= 0 || BlockAddress + BlockCount * BlockSize > reader.BaseStream.Length)
                {
                    IsEnabled = false;
                    return;
                }

                blockIndex = 0;
                foreach (XmlNode n in node.ChildNodes)
                    Children.Add(MetaValue.GetValue(n, cache, BlockAddress));

                RaisePropertyChanged(nameof(BlockIndex));

                var entryOffset = GetIntAttribute(node, "entryName", "entryOffset", "label");
                var entry = Children.FirstOrDefault(c => c.Offset == entryOffset);

                if (entry == null)
                    BlockLabels = Enumerable.Range(0, Math.Min(BlockCount, 100)).Select(i => $"Block {i:D3}");
                else
                {
                    var labels = new List<string>();
                    for (int i = 0; i < BlockCount; i++)
                    {
                        entry.BaseAddress = BlockAddress + i * BlockSize;
                        entry.ReadValue(reader);
                        labels.Add(entry.EntryString);
                    }
                    BlockLabels = labels;
                }
            }
            catch { IsEnabled = false; }
        }

        private void RefreshChildren()
        {
            if (BlockCount <= 0)
                return;

            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                foreach (var c in Children)
                {
                    c.BaseAddress = BlockAddress + BlockIndex * BlockSize;
                    c.ReadValue(reader);
                }
            }
        }
    }

    public class SimpleValue : MetaValue
    {
        public override string EntryString => Value.ToString();

        private object _value;
        public object Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public SimpleValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            ReadValue(reader);
        }

        internal override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                switch (ValueType)
                {
                    case MetaValueType.Int8: Value = reader.ReadByte(); break;
                    case MetaValueType.Int16: Value = reader.ReadInt16(); break;
                    case MetaValueType.Int32: Value = reader.ReadInt32(); break;
                    case MetaValueType.Int64: Value = reader.ReadInt64(); break;
                    case MetaValueType.UInt16: Value = reader.ReadUInt16(); break;
                    case MetaValueType.UInt32: Value = reader.ReadUInt32(); break;
                    case MetaValueType.UInt64: Value = reader.ReadUInt64(); break;
                    case MetaValueType.Float32: Value = reader.ReadSingle(); break;

                    case MetaValueType.Comment: Value = node.InnerText; break;

                    case MetaValueType.Undefined:
                    default:
                        Value = reader.ReadInt32();
                        break;
                }
            }
            catch { IsEnabled = false; }
        }
    }

    public class StringValue : MetaValue
    {
        public override string EntryString => Value;

        public int Length { get; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public StringValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
            : base(node, cache, baseAddress, reader)
        {
            Length = GetIntAttribute(node, "length", "maxlength", "size") ?? 0;
            ReadValue(reader);
        }

        internal override void ReadValue(EndianReader reader)
        {
            IsEnabled = true;

            try
            {
                reader.Seek(ValueAddress, SeekOrigin.Begin);

                if (ValueType == MetaValueType.String)
                    Value = reader.ReadNullTerminatedString();
                else
                {
                    var id = cache.CacheType < CacheType.Halo3Beta ? reader.ReadInt16() : reader.ReadInt32();
                    Value = cache.StringIndex[id];
                }
            }
            catch { IsEnabled = false; }
        }
    }

    public enum MetaValueType
    {
        [MetaValueTypeAlias("struct")]
        [MetaValueTypeAlias("reflexive")]
        Structure,

        StructureGroup,

        [MetaValueTypeAlias("tagreference")]
        TagRef,

        StringId,

        [MetaValueTypeAlias("ascii")]
        String,

        [MetaValueTypeAlias("bitfield8")]
        Bitmask8,

        [MetaValueTypeAlias("bitfield16")]
        Bitmask16,

        [MetaValueTypeAlias("bitfield32")]
        Bitmask32,

        Comment,

        DataRef,

        [MetaValueTypeAlias("float")]
        Float32,

        [MetaValueTypeAlias("sbyte")]
        Int8,

        [MetaValueTypeAlias("short")]
        Int16,

        [MetaValueTypeAlias("int")]
        Int32,

        [MetaValueTypeAlias("long")]
        Int64,

        [MetaValueTypeAlias("byte")]
        UInt8,

        [MetaValueTypeAlias("ushort")]
        UInt16,

        [MetaValueTypeAlias("uint")]
        UInt32,

        [MetaValueTypeAlias("ulong")]
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
                case MetaValueType.Structure:
                    return element.FindResource("StructureTemplate") as DataTemplate;

                default:
                    return element.FindResource("DefaultTemplate") as DataTemplate;
            }
        }
    }
}
