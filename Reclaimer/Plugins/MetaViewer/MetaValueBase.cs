using Reclaimer.IO;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer
{
    public abstract class MetaValueBase : BindableBase
    {
        protected readonly XmlNode Node;

        public virtual string Name { get; }
        public virtual int Offset { get; protected init; }
        public virtual string ToolTip { get; }
        public virtual string Description { get; }
        public virtual bool IsVisible { get; protected init; }

        public long BaseAddress { get; internal set; }

        public FieldDefinition FieldDefinition { get; protected init; }

        public long ValueAddress => BaseAddress + Offset;

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
        }

        public virtual string EntryString => null;

        public bool IsBusy { get; protected set; }
        public bool IsDirty { get; protected set; }

        protected MetaValueBase(XmlNode node, long baseAddress)
        {
            Node = node;

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
            if (changed)
            {
                IsDirty = true;
                if (!IsBusy && MetaValidationRule.Validate(this, value))
                    OnMetaPropertyChanged(propertyName);
            }
            return changed;
        }

        protected virtual void OnMetaPropertyChanged(string propertyName) { }

        public abstract void ReadValue(EndianReader reader);

        public abstract void WriteValue(EndianWriter writer);

        public static Halo3.MetaValue GetMetaValue(XmlNode node, Halo3.MetaContext context, long baseAddress)
        {
            using (var reader = context.CreateReader())
            {
                reader.Seek(baseAddress, SeekOrigin.Begin);

                var def = FieldDefinition.GetHalo3Definition(node);

                if (def.Components > 1)
                {
                    switch (def.ValueType)
                    {
                        case MetaValueType.Float32:
                            return new Halo3.MultiValue<float>(node, context, reader, baseAddress);
                        case MetaValueType.Byte:
                            return new Halo3.MultiValue<byte>(node, context, reader, baseAddress);
                    }
                }

                switch (def.ValueType)
                {
                    case MetaValueType.Structure:
                        return new Halo3.StructureValue(node, context, reader, baseAddress);

                    case MetaValueType.String:
                        return new Halo3.StringValue(node, context, reader, baseAddress);

                    case MetaValueType.StringId:
                        return new Halo3.StringIdValue(node, context, reader, baseAddress);

                    case MetaValueType.TagReference:
                        return new Halo3.TagReferenceValue(node, context, reader, baseAddress);

                    case MetaValueType.Revisions:
                    case MetaValueType.Comment:
                        return new Halo3.CommentValue(node, context, reader, baseAddress);

                    case MetaValueType.Bitmask8:
                    case MetaValueType.Bitmask16:
                    case MetaValueType.Bitmask32:
                        return new Halo3.BitmaskValue(node, context, reader, baseAddress);

                    case MetaValueType.Enum8:
                    case MetaValueType.Enum16:
                    case MetaValueType.Enum32:
                        return new Halo3.EnumValue(node, context, reader, baseAddress);

                    case MetaValueType.BlockIndex8:
                    case MetaValueType.BlockIndex16:
                    case MetaValueType.BlockIndex32:
                        return new Halo3.BlockIndexValue(node, context, reader, baseAddress);

                    default:
                        return new Halo3.SimpleValue(node, context, reader, baseAddress);
                }
            }
        }

        public static Halo5.MetaValue GetMetaValue(XmlNode node, Blam.Common.Gen5.IModuleItem item, Blam.Common.Gen5.IMetadataHeader header, Blam.Common.Gen5.DataBlock host, EndianReader reader, long baseAddress, int offset)
        {
            reader.Seek(baseAddress, SeekOrigin.Begin);

            var def = FieldDefinition.GetHalo5Definition(item, node);

            if (def.Size < 0)
                System.Diagnostics.Debugger.Break();

            if (def.Components > 1)
            {
                switch (def.ValueType)
                {
                    case MetaValueType.Float32:
                        return new Halo5.MultiValue<float>(node, item, header, host, reader, baseAddress, offset);
                    case MetaValueType.Byte:
                        return new Halo5.MultiValue<byte>(node, item, header, host, reader, baseAddress, offset);
                    case MetaValueType.Int16:
                        return new Halo5.MultiValue<short>(node, item, header, host, reader, baseAddress, offset);
                }
            }

            switch (def.ValueType)
            {
                case MetaValueType.Structure:
                    return new Halo5.StructureValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.Array:
                    return new Halo5.ArrayValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.StringId:
                case MetaValueType.String:
                    return new Halo5.StringValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.TagReference:
                    return new Halo5.TagReferenceValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.Comment:
                    return new Halo5.CommentValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.Bitmask32:
                case MetaValueType.Bitmask16:
                case MetaValueType.Bitmask8:
                    return new Halo5.BitmaskValue(node, item, header, host, reader, baseAddress, offset);

                case MetaValueType.Enum32:
                case MetaValueType.Enum16:
                case MetaValueType.Enum8:
                    return new Halo5.EnumValue(node, item, header, host, reader, baseAddress, offset);

                default:
                    return new Halo5.SimpleValue(node, item, header, host, reader, baseAddress, offset);
            }
        }

        protected internal virtual bool HasCustomValidation => false;

        protected internal virtual bool ValidateValue(object value)
        {
            throw new NotImplementedException();
        }

        public virtual Newtonsoft.Json.Linq.JToken GetJValue() => null;
    }
}
