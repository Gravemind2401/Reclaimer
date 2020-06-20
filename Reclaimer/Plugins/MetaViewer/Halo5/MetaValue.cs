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

        public FieldDefinition FieldDefinition { get; }
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
            Description = node.GetStringAttribute("description", "desc");
            ToolTip = node.GetStringAttribute("tooltip");
            IsVisible = true;// node.GetBoolAttribute("visible") ?? false;

            FieldDefinition = FieldDefinition.GetDefinition(node);
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

            var def = FieldDefinition.GetDefinition(node);

            if (def.Components > 1 && def.ValueType == MetaValueType.Float32)
                return new MultiValue(node, item, header, reader, baseAddress, offset);

            switch (def.ValueType)
            {
                case MetaValueType.Structure:
                    return new StructureValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.StringId:
                case MetaValueType.String:
                    return new StringValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Comment:
                    return new CommentValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Bitmask32:
                case MetaValueType.Bitmask16:
                case MetaValueType.Bitmask8:
                    return new BitmaskValue(node, item, header, reader, baseAddress, offset);

                case MetaValueType.Enum32:
                case MetaValueType.Enum16:
                case MetaValueType.Enum8:
                    return new EnumValue(node, item, header, reader, baseAddress, offset);

                default:
                    return new SimpleValue(node, item, header, reader, baseAddress, offset);
            }
        }

        public abstract void ReadValue(EndianReader reader);

        public abstract void WriteValue(EndianWriter writer);
    }
}
