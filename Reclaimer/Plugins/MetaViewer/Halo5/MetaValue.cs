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
        protected readonly ModuleItem item;
        protected readonly MetadataHeader header;

        public override int Offset { get; }
        public override bool IsVisible { get; }

        public override FieldDefinition FieldDefinition { get; }

        protected MetaValue(XmlNode node, ModuleItem item, MetadataHeader header, EndianReader reader, long baseAddress, int offset)
            : base(node, baseAddress)
        {
            this.item = item;
            this.header = header;

            Offset = offset;
            IsVisible = true;

            FieldDefinition = FieldDefinition.GetHalo5Definition(node);
        }
    }
}
