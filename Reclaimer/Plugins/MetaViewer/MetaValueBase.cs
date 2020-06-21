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
using Adjutant.Blam.Common;

namespace Reclaimer.Plugins.MetaViewer
{
    public abstract class MetaValueBase : BindableBase
    {
        protected readonly XmlNode node;

        public virtual string Name { get; }
        public virtual int Offset { get; }
        public virtual string ToolTip { get; }
        public virtual string Description { get; }
        public virtual bool IsVisible { get; }

        public long BaseAddress { get; internal set; }

        public abstract FieldDefinition FieldDefinition { get; }
        public long ValueAddress => BaseAddress + Offset;

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        public virtual string EntryString => null;

        public bool IsDirty { get; protected set; }

        protected MetaValueBase(XmlNode node, long baseAddress)
        {
            this.node = node;

            BaseAddress = baseAddress;

            Name = node.Attributes["name"]?.Value;
            Description = node.GetStringAttribute("description", "desc");
            Offset = node.GetIntAttribute("offset") ?? 0;
            ToolTip = node.GetStringAttribute("tooltip");
            IsVisible = node.GetBoolAttribute("visible") ?? false;
        }

        protected bool SetMetaProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            var changed = SetProperty(ref property, value, propertyName);
            if (changed) IsDirty = true;
            return changed;
        }

        public abstract void ReadValue(EndianReader reader);

        public abstract void WriteValue(EndianWriter writer);

        public static MetaValueBase GetMetaValue(XmlNode node, ICacheFile cache, long baseAddress)
        {
            using (var reader = cache.CreateReader(cache.DefaultAddressTranslator))
            {
                reader.Seek(baseAddress, SeekOrigin.Begin);

                var def = FieldDefinition.GetHalo3Definition(node);

                if (def.Components > 1 && def.ValueType == MetaValueType.Float32)
                    return new Halo3.MultiValue(node, cache, reader, baseAddress);

                switch (def.ValueType)
                {
                    case MetaValueType.Structure:
                        return new Halo3.StructureValue(node, cache, reader, baseAddress);

                    case MetaValueType.String:
                    case MetaValueType.StringId:
                        return new Halo3.StringValue(node, cache, reader, baseAddress);

                    case MetaValueType.TagReference:
                        return new Halo3.TagReferenceValue(node, cache, reader, baseAddress);

                    case MetaValueType.Revisions:
                    case MetaValueType.Comment:
                        return new Halo3.CommentValue(node, cache, reader, baseAddress);

                    case MetaValueType.Bitmask8:
                    case MetaValueType.Bitmask16:
                    case MetaValueType.Bitmask32:
                        return new Halo3.BitmaskValue(node, cache, reader, baseAddress);

                    case MetaValueType.Enum8:
                    case MetaValueType.Enum16:
                    case MetaValueType.Enum32:
                        return new Halo3.EnumValue(node, cache, reader, baseAddress);

                    default: return new Halo3.SimpleValue(node, cache, reader, baseAddress);
                }
            }
        }

        public static MetaValueBase GetMetaValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
        {
            reader.Seek(baseAddress, SeekOrigin.Begin);

            var def = FieldDefinition.GetHalo5Definition(node);

            if (def.Components > 1 && def.ValueType == MetaValueType.Float32)
                return new Halo5.MultiValue(node, item, header, reader, baseAddress, offset);

            switch (def.ValueType)
            {
                case MetaValueType.Structure:
                    return new Halo5.StructureValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.StringId:
                case MetaValueType.String:
                    return new Halo5.StringValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Comment:
                    return new Halo5.CommentValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Bitmask32:
                case MetaValueType.Bitmask16:
                case MetaValueType.Bitmask8:
                    return new Halo5.BitmaskValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Enum32:
                case MetaValueType.Enum16:
                case MetaValueType.Enum8:
                    return new Halo5.EnumValue(node, item, header, reader, baseAddress, offset);

                default:
                    return new Halo5.SimpleValue(node, item, header, reader, baseAddress, offset);
            }
        }
    }
}
