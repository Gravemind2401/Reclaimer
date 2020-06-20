using Adjutant.Blam.Halo5;
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
using Prism.Mvvm;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public abstract class MetaValue : BindableBase
    {
        protected readonly ModuleItem item;
        protected readonly MetadataHeader header;
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

        protected MetaValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
        {
            this.item = item;
            this.header = header;
            this.node = node;

            BaseAddress = baseAddress;
            Offset = offset;

            Name = node.Attributes["name"]?.Value;
            Description = GetStringAttribute(node, "description", "desc");
            ToolTip = GetStringAttribute(node, "tooltip");
            IsVisible = true;// GetBoolAttribute(node, "visible") ?? false;

            ValueType = GetMetaValueType(node.Name);
        }

        protected bool SetMetaProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = SetProperty(ref property, value, propertyName);
            if (changed) IsDirty = true;
            return changed;
        }

        public static MetaValue GetValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
        {
            reader.Seek(baseAddress, SeekOrigin.Begin);

            MetaValue result;
            switch (GetMetaValueType(node.Name))
            {
                case MetaValueType._field_block_64:
                    return new StructureValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType._field_string:
                case MetaValueType._field_long_string:
                case MetaValueType._field_string_id:
                    return new StringValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType._field_explanation:
                    return new CommentValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType._field_byte_flags:
                case MetaValueType._field_word_flags:
                case MetaValueType._field_long_flags:
                case MetaValueType._field_long_block_flags:
                    return new BitmaskValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType._field_char_enum:
                case MetaValueType._field_short_enum:
                case MetaValueType._field_long_enum:
                    return new EnumValue(node, item, header, reader, baseAddress, offset);

                default:
                    result = new SimpleValue(node, item, header, reader, baseAddress, offset);
                    break;
            }

            return result;
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

        protected static MetaValueType GetMetaValueType(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException(nameof(typeName));

            MetaValueType result;
            if (Enum.TryParse(typeName, true, out result))
                return result;
            else throw new NotSupportedException();
        }

        public static int CalculateSize(XmlNode node)
        {
            switch (GetMetaValueType(node.Name))
            {
                case MetaValueType._field_struct:
                case MetaValueType._field_array:
                    return node.ChildNodes.OfType<XmlNode>().Sum(n => CalculateSize(n));

                case MetaValueType._field_pad:
                case MetaValueType._field_skip:
                    return int.Parse(node.Attributes["length"].Value);

                case MetaValueType._field_tag_reference_64:
                    return 32;

                case MetaValueType._field_block_64:
                case MetaValueType._field_data_64:
                    return 28;

                case MetaValueType._field_long_string:
                    return 256;

                case MetaValueType._field_string:
                    return 32;

                case MetaValueType._field_pageable_resource_64:
                case MetaValueType._field_real_quaternion:
                case MetaValueType._field_real_argb_color:
                    return 16;

                case MetaValueType._field_real_point_3d:
                case MetaValueType._field_real_vector_3d:
                case MetaValueType._field_real_plane_3d:
                case MetaValueType._field_real_euler_angles_3d:
                case MetaValueType._field_real_rgb_color:
                    return 12;

                case MetaValueType._field_angle_bounds:
                case MetaValueType._field_real_bounds:
                case MetaValueType._field_real_point_2d:
                case MetaValueType._field_real_euler_angles_2d:
                case MetaValueType._field_int64_integer:
                case MetaValueType._field_qword_integer:
                    return 8;

                case MetaValueType._field_real:
                case MetaValueType._field_real_fraction:
                case MetaValueType._field_angle:
                case MetaValueType._field_long_enum:
                case MetaValueType._field_long_flags:
                case MetaValueType._field_long_integer:
                case MetaValueType._field_dword_integer:
                case MetaValueType._field_long_block_index:
                case MetaValueType._field_long_block_flags:
                case MetaValueType._field_short_integer_bounds:
                case MetaValueType._field_point_2d:
                case MetaValueType._field_string_id:
                    return 4;

                case MetaValueType._field_short_enum:
                case MetaValueType._field_word_flags:
                case MetaValueType._field_word_integer:
                case MetaValueType._field_short_integer:
                case MetaValueType._field_short_block_index:
                    return 2;

                case MetaValueType._field_char_enum:
                case MetaValueType._field_byte_flags:
                case MetaValueType._field_byte_integer:
                case MetaValueType._field_char_integer:
                    return 1;

                case MetaValueType._field_explanation:
                case MetaValueType._field_custom:
                    return 0;

                default: throw new NotSupportedException();
            }
        }
    }
}
