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
using Prism.Mvvm;

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

        protected MetaValue(XmlNode node, ICacheFile cache, long baseAddress, EndianReader reader)
        {
            this.cache = cache;
            this.node = node;

            BaseAddress = baseAddress;

            Name = node.Attributes["name"]?.Value;
            Description = node.GetStringAttribute("description", "desc");
            Offset = node.GetIntAttribute("offset") ?? 0;
            ToolTip = node.GetStringAttribute("tooltip");
            IsVisible = node.GetBoolAttribute("visible") ?? false;

            FieldDefinition = FieldDefinition.GetDefinition(node);
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

                var def = FieldDefinition.GetDefinition(node);

                if (def.Components > 1 && def.ValueType == MetaValueType.Float32)
                    return new MultiValue(node, cache, baseAddress, reader);

                switch (def.ValueType)
                {
                    case MetaValueType.Structure:
                        return new StructureValue(node, cache, baseAddress, reader);

                    case MetaValueType.String:
                    case MetaValueType.StringId:
                        return new StringValue(node, cache, baseAddress, reader);

                    case MetaValueType.TagReference:
                        return new TagReferenceValue(node, cache, baseAddress, reader);

                    case MetaValueType.Revisions:
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

                    default: return new SimpleValue(node, cache, baseAddress, reader);
                }
            }
        }

        public abstract void ReadValue(EndianReader reader);

        public abstract void WriteValue(EndianWriter writer);
    }
}
