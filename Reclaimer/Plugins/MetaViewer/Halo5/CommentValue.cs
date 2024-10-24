using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo5
{
    public class CommentValue : MetaValue
    {
        public string Title { get; }
        public string Body { get; }

        public CommentValue(XmlNode node, IModuleItem item, IMetadataHeader header, DataBlock host, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, host, reader, baseAddress, offset)
        {
            Title = node.GetStringAttribute("title", "name");
            Body = node.InnerText;
        }

        public override void ReadValue(EndianReader reader)
        {

        }

        public override void WriteValue(EndianWriter writer)
        {

        }
    }
}
