using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Reclaimer.IO;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public class CommentValue : MetaValue
    {
        public string Title { get; }
        public string Body { get; }

        public CommentValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, context, reader, baseAddress)
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
