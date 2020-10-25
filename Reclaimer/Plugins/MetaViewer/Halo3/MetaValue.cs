using Adjutant.Blam.Common;
using System;
using System.Collections.Generic;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Reclaimer.Plugins.MetaViewer.Halo3
{
    public abstract class MetaValue : MetaValueBase
    {
        protected readonly MetaContext context;

        public override FieldDefinition FieldDefinition { get; }

        protected MetaValue(XmlNode node, MetaContext context, EndianReader reader, long baseAddress)
            : base(node, baseAddress)
        {
            this.context = context;
            FieldDefinition = FieldDefinition.GetHalo3Definition(node);
            context.AddValue(node, this);
        }

        protected override void OnMetaPropertyChanged(string propertyName)
        {
            using (var writer = new EndianWriter(context.DataSource, context.Cache.ByteOrder, true))
                WriteValue(writer);
        }
    }
}
