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
    public class CommentValue : MetaValue
    {
        public string Title { get; }
        public string Body { get; }

        public CommentValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, item, header, reader, baseAddress, offset)
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
