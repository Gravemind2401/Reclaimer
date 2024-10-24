using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Halo5;
using Reclaimer.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public abstract class MetaValue : MetaValueBase
    {
        private readonly NameHelper nameHelper;

        protected IModuleItem Item { get; }
        protected IMetadataHeader Header { get; }
        protected DataBlock Host { get; }

        public override string Name => nameHelper.Name;
        public override string ToolTip => nameHelper.ToolTip;
        public override string Description => nameHelper.Description;
        public bool IsBlockName => nameHelper.IsBlockName;

        protected MetaValue(XmlNode node, IModuleItem item, IMetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, baseAddress)
        {
            Item = item;
            Header = header;
            Host = host;

            FieldDefinition = FieldDefinition.GetHalo5Definition(item, node);

            nameHelper = new NameHelper(node.GetStringAttribute("name"));

            Offset = offset;
            IsVisible = !FieldDefinition.Hidden;
        }
    }
}
