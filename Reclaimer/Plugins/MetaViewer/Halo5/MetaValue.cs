using Reclaimer.Blam.Halo5;
using Reclaimer.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public abstract class MetaValue : MetaValueBase
    {
        //^*~!&#
        private const string specialChars = "^:#";

        private readonly NameHelper nameHelper;

        protected readonly ModuleItem item;
        protected readonly MetadataHeader header;
        protected readonly DataBlock host;

        public override string Name => nameHelper.Name;
        public override string ToolTip => nameHelper.ToolTip;
        public override string Description => nameHelper.Description;
        public bool IsBlockName => nameHelper.IsBlockName;

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

            nameHelper = new NameHelper(node.GetStringAttribute("name"));

            Offset = offset;
            IsVisible = !FieldDefinition.Hidden;
        }
    }
}
