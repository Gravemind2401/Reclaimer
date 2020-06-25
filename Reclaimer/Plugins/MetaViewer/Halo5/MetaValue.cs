using Adjutant.Blam.Halo5;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public abstract class MetaValue : MetaValueBase
    {
        //^*~!&#
        private const string specialChars = "^:#";

        protected readonly ModuleItem item;
        protected readonly MetadataHeader header;
        protected readonly DataBlock host;

        public override string Name { get; }
        public override string ToolTip { get; }
        public override string Description { get; }

        public bool IsBlockName { get; }

        public override int Offset { get; }
        public override bool IsVisible { get; }

        public override FieldDefinition FieldDefinition { get; }

        protected MetaValue(XmlNode node, ModuleItem item, MetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, baseAddress)
        {
            this.item = item;
            this.header = header;
            this.host = host;

            FieldDefinition = FieldDefinition.GetHalo5Definition(node);

            var rawName = node.GetStringAttribute("name");
            Name = GetName(rawName);
            Description = GetDescription(rawName);
            ToolTip = GetTooltip(rawName);

            IsBlockName = rawName.Contains('^');

            Offset = offset;
            IsVisible = FieldDefinition.ValueType != MetaValueType.Padding;
        }

        private string GetName(string value)
        {
            return value.Split(specialChars.ToArray()).First();
        }

        private string GetDescription(string value)
        {
            var start = value.IndexOf(':');
            if (start < 0)
                return null;

            var end = value.IndexOf('#');
            if (end < 0)
                end = value.Length;

            return value.Substring(start + 1, end - (start + 1));
        }

        private string GetTooltip(string value)
        {
            var start = value.IndexOf('#');
            if (start < 0)
                return null;

            return value.Substring(start + 1);
        }
    }
}
